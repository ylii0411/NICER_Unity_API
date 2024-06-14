using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace NICER_Unity_API
{
    public class NICER_API : MonoBehaviour
    {

        private NICER_Model nicer = new NICER_Model();

        public double[] generatePrediction(Transform hand, Transform wrist, Transform elbow, Transform shoulder, string gender, float delta, float duration)
        {
            double[] fatigue_indicator;

            nicer.SetGenderValue(gender);

            nicer.rawTorque = nicer.EstimateInsTorque(hand, wrist, elbow, shoulder, delta);

            // Determine if the participant is resting
            if (nicer.thetaShoulder <= 25 && nicer.thetaElbow >= 125)
                nicer.status_current = 0; // Interaction is paused for a break
            else
                nicer.status_current = 1; // Interaction is undergoing

            // Determine if the study status is updated
            if (nicer.status_current > nicer.status_last) // from rest to active
            {
                nicer.revisedAvgTorque = nicer.CalculateReducedTorque(nicer.chaffin_last, nicer.fatigue_last, delta, duration);

                if (nicer.thetaShoulder >= 90 && nicer.thetaElbow >= 145)
                {
                    nicer.correcTerm = nicer.CalculateCorrectionTerm(nicer.thetaShoulder);
                }
                else
                    nicer.correcTerm = 0;

                nicer.revisedAvgTorque = nicer.CalculateRevisedAvgTorque(nicer.rawTorque, nicer.correcTerm, nicer.revisedAvgTorque, delta, duration);

                nicer.chaffin_current = nicer.EstimateChaffinStrength(nicer.thetaShoulder, nicer.thetaElbow);

                nicer.avgTorquePercent = nicer.CalculateRevisedMVC(nicer.revisedAvgTorque, nicer.chaffin_current);
                 
                fatigue_indicator = nicer.EstimateFatiguePrediction(nicer.avgTorquePercent, duration);
                nicer.fatigue_current = fatigue_indicator[1];
            }
            else if (nicer.status_current < nicer.status_last) // from active to rest
            {
                //nicer.fatigue_current = nicer.EstimateReducedFatigue(nicer.fatigue_last, delta);
                fatigue_indicator = nicer.EstimateReducedFatigue(nicer.fatigue_last, delta, duration);
                nicer.fatigue_current = fatigue_indicator[1];
            }
            else if (nicer.status_current == 0 && nicer.status_last == 0) // keep resting
            {
                //nicer.fatigue_current = nicer.EstimateReducedFatigue(nicer.fatigue_last, delta);
                fatigue_indicator = nicer.EstimateReducedFatigue(nicer.fatigue_last, delta, duration);
                nicer.fatigue_current = fatigue_indicator[1];
            }
            else // nicer.status_current == nicer.status_last == 1 // keep working
            {
                if (nicer.thetaShoulder >= 90 && nicer.thetaElbow >= 145)
                {
                    nicer.correcTerm = nicer.CalculateCorrectionTerm(nicer.thetaShoulder);
                }
                else
                    nicer.correcTerm = 0;

                nicer.revisedAvgTorque = nicer.CalculateRevisedAvgTorque(nicer.rawTorque, nicer.correcTerm, nicer.revisedAvgTorque, delta, duration);

                nicer.chaffin_current = nicer.EstimateChaffinStrength(nicer.thetaShoulder, nicer.thetaElbow);

                nicer.avgTorquePercent = nicer.CalculateRevisedMVC(nicer.revisedAvgTorque, nicer.chaffin_current);

                fatigue_indicator = nicer.EstimateFatiguePrediction(nicer.avgTorquePercent, duration);
                nicer.fatigue_current = fatigue_indicator[1];
            }

            nicer.chaffin_last = nicer.chaffin_current;

            nicer.fatigue_last = nicer.fatigue_current;

            nicer.status_last = nicer.status_current;

            return fatigue_indicator;
        }
    }

    public class NICER_Model
    {

        //Constant Property
        private const double HAND_MASS = 0.4;
        private const double MALE_FOREARM_MASS = 1.2;
        private const double FEMALE_FOREARM_MASS = 1.0;

        private const double MALE_UPPERARM_MASS = 2.1;
        private const double FEMALE_UPPERARM_MASS = 1.7;

        private const double MALE_UPPERARM_LENGHT = 33;
        private const double MALE_FOREARM_LENGHT = 26.9;
        private const double MALE_HAND_LENGHT = 19.1;

        private const double FEMALE_UPPERARM_LENGHT = 31;
        private const double FEMALE_FOREARM_LENGHT = 23.4;
        private const double FEMALE_HAND_LENGHT = 18.3;

        private const double UPPER_ARM_CENTER_GRAVITY_RATIO = 0.452;
        private const double MALE_UPPER_ARM_CENTER_OF_GRAVITY = MALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO;
        private const double FEMALE_UPPER_ARM_CENTER_OF_GRAVITY = FEMALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO;

        private const double FOREARM_CENTER_GRAVITY_RATIO = 0.424;
        private const double MALE_FOREARM_CENTER_OF_GRAVITY = MALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO;
        private const double FEMALE_FOREARM_CENTER_OF_GRAVITY = FEMALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO;

        private const double HAND_CENTER_GRAVITY_RATIO = 0.397;
        private const double MALE_HAND_CENTER_OF_GRAVITY = MALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO;
        private const double FEMALE_HAND_CENTER_OF_GRAVITY = FEMALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO;

        private const double UPPER_ARM_INERTIA_RATE = 0.0141;     //141*10^(-4)kg
        private const double FORE_ARM_INERTIA_RATE = 0.0055;    //55*10^(-4)kg

        private const double GRAVITY_ACCELERATION = -9.8; // m/s

        private const double MALE_MAX_FORCE = 101.6;
        private const double FEMALE_MAX_FORCE = 87.2;
        private const double MAX_ENDURANCE = 1.7976931348623157E+308;

        private static readonly Vector3 GRAVITY_VECTOR = new Vector3(0, (float)GRAVITY_ACCELERATION, 0);

        //Private Variables
        private Dictionary<string, Vector3> armJointPoints;
        private Vector3 CenterOfMass;

        private double armMass;
        private double upperArmWeightProportion; //Segment weight at the 50th percentile, male
        private double forearmAndHandCenterOfGravity;
        private double foreArmAndHandCenterOfGravityRatio;

        private double maxForce;
        private double maxTorque;

        private Vector3 armCM;
        private Vector3 armLastCM;
        private Vector3 upperCM;
        private Vector3 foreCM;

        private double theta = 0;

        private Vector3 displacement;
        private Vector3 currentVelocity;
        private Vector3 lastVelocity;
        private Vector3 measuredAcceleration;

        private double angularAcc;
        private Vector3 inertialTorque;

        //Define elbow and shoulder angles
        public double thetaElbow;
        public double thetaShoulder;

        public double rawTorque;
        public double revisedAvgTorque;
        public double avgTorquePercent;

        public double correcTerm;

        public double chaffin_last;
        public double chaffin_current;

        // status = 0: rest; status = 1: active.
        public int status_current;
        public int status_last = 1;

        public string gender;

        public double fatigue_current;
        public double fatigue_last;

        public void SetGenderValue(string gd)
        {
            gender = gd;

            if (gd == "Male")
            {
                armMass = MALE_UPPERARM_MASS + MALE_FOREARM_MASS + HAND_MASS;
                maxForce = MALE_MAX_FORCE;
                maxTorque = maxForce * MALE_UPPERARM_LENGHT / 100 +
                                        (MALE_UPPERARM_MASS * GRAVITY_ACCELERATION * MALE_UPPER_ARM_CENTER_OF_GRAVITY / 100 +
                                        MALE_FOREARM_MASS * GRAVITY_ACCELERATION * (MALE_UPPERARM_LENGHT + MALE_FOREARM_CENTER_OF_GRAVITY) / 100 +
                                        HAND_MASS * GRAVITY_ACCELERATION * (MALE_UPPERARM_LENGHT + MALE_FOREARM_LENGHT + MALE_HAND_CENTER_OF_GRAVITY) / 100);

                upperArmWeightProportion = MALE_UPPERARM_MASS / armMass;//Segment weight at the 50th percentile, male
                forearmAndHandCenterOfGravity = ((MALE_FOREARM_LENGHT + MALE_HAND_CENTER_OF_GRAVITY) - MALE_FOREARM_CENTER_OF_GRAVITY) * (HAND_MASS / (MALE_FOREARM_MASS + HAND_MASS)) + MALE_FOREARM_CENTER_OF_GRAVITY;
                foreArmAndHandCenterOfGravityRatio = forearmAndHandCenterOfGravity / (MALE_FOREARM_LENGHT + MALE_HAND_LENGHT);

            }
            else
            {
                armMass = FEMALE_UPPERARM_MASS + FEMALE_FOREARM_MASS + HAND_MASS;
                maxForce = FEMALE_MAX_FORCE;
                maxTorque = maxForce * FEMALE_UPPERARM_LENGHT / 100 +
                                        (FEMALE_UPPERARM_MASS * GRAVITY_ACCELERATION * FEMALE_UPPER_ARM_CENTER_OF_GRAVITY / 100 +
                                        FEMALE_FOREARM_MASS * GRAVITY_ACCELERATION * (FEMALE_UPPERARM_LENGHT + FEMALE_FOREARM_CENTER_OF_GRAVITY) / 100 +
                                        HAND_MASS * GRAVITY_ACCELERATION * (FEMALE_UPPERARM_LENGHT + FEMALE_FOREARM_LENGHT + FEMALE_HAND_CENTER_OF_GRAVITY) / 100);

                upperArmWeightProportion = FEMALE_UPPERARM_MASS / armMass;//Segment weight at the 50th percentile, female
                forearmAndHandCenterOfGravity = ((FEMALE_FOREARM_LENGHT + FEMALE_HAND_CENTER_OF_GRAVITY) - FEMALE_FOREARM_CENTER_OF_GRAVITY) * (HAND_MASS / (FEMALE_FOREARM_MASS + HAND_MASS)) + FEMALE_FOREARM_CENTER_OF_GRAVITY;
                foreArmAndHandCenterOfGravityRatio = forearmAndHandCenterOfGravity / (FEMALE_FOREARM_LENGHT + FEMALE_HAND_LENGHT);
            }
        }

        private Dictionary<string, Vector3> GetArmPoints(Transform rightHand, Transform rightWrist, Transform rightElbow, Transform rightShoulder)
        {
            Dictionary<string, Vector3> tempJointPoints = new Dictionary<string, Vector3>();

            Vector3 hand = new Vector3(
                rightHand.position.x,
                rightHand.position.y,
                rightHand.position.z);

            if (rightWrist.position.x != 0 && rightWrist.position.y != 0 && rightWrist.position.z != 0)
            {
                hand = new Vector3(
                (float)0.397 * (rightHand.position.x - rightWrist.position.x) + rightWrist.position.x,
                (float)0.397 * (rightHand.position.y - rightWrist.position.y) + rightWrist.position.y,
                (float)0.397 * (rightHand.position.z - rightWrist.position.z) + rightWrist.position.z);
            }
            

            Vector3 elbow = new Vector3(
                rightElbow.position.x,
                rightElbow.position.y,
                rightElbow.position.z);

            Vector3 shoulder = new Vector3(
                rightShoulder.position.x,
                rightShoulder.position.y,
                rightShoulder.position.z);

            tempJointPoints.Add("Hand", new Vector3(hand.x - shoulder.x, hand.y - shoulder.y, hand.z - shoulder.z));
            tempJointPoints.Add("Elbow", new Vector3(elbow.x - shoulder.x, elbow.y - shoulder.y, elbow.z - shoulder.z));
            tempJointPoints.Add("Shoulder", new Vector3(0, 0, 0));

            return tempJointPoints;
        }

        private Vector3 CalculateCenterMass(Vector3 shoulder, Vector3 elbow, Vector3 hand)
        {

            //upper arm center mass and fore arm center mass
            upperCM.x = (elbow.x - shoulder.x) * (float)UPPER_ARM_CENTER_GRAVITY_RATIO;
            upperCM.y = (elbow.y - shoulder.y) * (float)UPPER_ARM_CENTER_GRAVITY_RATIO;
            upperCM.z = (elbow.z - shoulder.z) * (float)UPPER_ARM_CENTER_GRAVITY_RATIO;

            //lower arm
            foreCM.x = (hand.x - elbow.x) * (float)foreArmAndHandCenterOfGravityRatio + elbow.x;
            foreCM.y = (hand.y - elbow.y) * (float)foreArmAndHandCenterOfGravityRatio + elbow.y;
            foreCM.z = (hand.z - elbow.z) * (float)foreArmAndHandCenterOfGravityRatio + elbow.z;

            //base on equation get whole arm center mass
            armCM.x = (foreCM.x - upperCM.x) * (1 - (float)upperArmWeightProportion) + upperCM.x;
            armCM.y = (foreCM.y - upperCM.y) * (1 - (float)upperArmWeightProportion) + upperCM.y;
            armCM.z = (foreCM.z - upperCM.z) * (1 - (float)upperArmWeightProportion) + upperCM.z;

            var normalizingFactor = MALE_UPPERARM_LENGHT / ((elbow - shoulder).magnitude * 100);

            if (maxForce == FEMALE_MAX_FORCE)
                normalizingFactor = FEMALE_UPPERARM_LENGHT / ((elbow - shoulder).magnitude * 100);

            var normalizedArmCM = armCM * (float)normalizingFactor;

            return normalizedArmCM;
        }

        private double CalculateAngularAcc(Vector3 acceleration, float radious)
        {
            return acceleration.magnitude / radious;
        }

        private Vector3 CalculateInertialTorque(Vector3 dis, Vector3 ac)
        {
            Vector3 inertialT = Vector3.Cross(dis, ac);

            if (inertialT.magnitude != 0)
                inertialT.Normalize();

            inertialT.x *= ((float)UPPER_ARM_INERTIA_RATE + (float)FORE_ARM_INERTIA_RATE) * (float)angularAcc;
            inertialT.y *= ((float)UPPER_ARM_INERTIA_RATE + (float)FORE_ARM_INERTIA_RATE) * (float)angularAcc;
            inertialT.z *= ((float)UPPER_ARM_INERTIA_RATE + (float)FORE_ARM_INERTIA_RATE) * (float)angularAcc;

            return inertialT;
        }

        private Vector3 CalculateDisplacement(Vector3 cu_cm, Vector3 ls_cm)
        {
            Vector3 dis = new Vector3();
            //calculate displacement
            //4.0- Calculate new displacement
            dis.x = cu_cm.x - ls_cm.x;
            dis.y = cu_cm.y - ls_cm.y;
            dis.z = cu_cm.z - ls_cm.z;

            return dis;
        }

        private Vector3 CalculateVelocity(Vector3 disp, float delta)
        {
            Vector3 velocity = new Vector3();
            //calculate velocity
            //4.1- Calculate new velocity
            if (delta != 0)
            {
                velocity.x = disp.x / delta;
                velocity.y = disp.y / delta;
                velocity.z = disp.z / delta;
            }
            return velocity;
        }

        private Vector3 CalculateMovingAcc(Vector3 currentV, Vector3 lastV, float delta)
        {
            Vector3 acceleration = new Vector3();
            //calculate velocity and acceleration vector
            //4.2- Calculate new acceleration
            if (delta != 0)
            {
                acceleration.x = (currentV.x - lastV.x) / delta;
                acceleration.y = (currentV.y - lastV.y) / delta;
                acceleration.z = (currentV.z - lastV.z) / delta;
            }
            return acceleration;
        }

        public double EstimateInsTorque(Transform rightHand, Transform rightWrist, Transform rightElbow, Transform rightShoulder, float delta)
        {
            armJointPoints = GetArmPoints(rightHand, rightWrist, rightElbow, rightShoulder);
            //total arm mass center
            CenterOfMass = CalculateCenterMass(armJointPoints["Shoulder"], armJointPoints["Elbow"], armJointPoints["Hand"]);

            //angel of shoulder-centermass vector with gravity acceleration
            theta = Mathf.PI * Vector3.Angle(armCM, GRAVITY_VECTOR) / 180;

            //4- Calculate movement acceleration

            if (!armLastCM.Equals(default))
            {
                displacement = CalculateDisplacement(armCM, armLastCM);

                //v
                currentVelocity = CalculateVelocity(displacement, delta);

                //a
                measuredAcceleration = CalculateMovingAcc(currentVelocity, lastVelocity, delta);

                //α
                angularAcc = CalculateAngularAcc(measuredAcceleration, CenterOfMass.magnitude);

                //Iα/m
                inertialTorque = CalculateInertialTorque(displacement, armCM);

                Vector3 forceAtCoM = measuredAcceleration * (float)armMass;
                Vector3 systemTorque = Vector3.Cross(forceAtCoM, CenterOfMass);
                Vector3 gravityForce = GRAVITY_VECTOR * (float)armMass;
                Vector3 gravityTorque = Vector3.Cross(gravityForce, CenterOfMass);
                Vector3 shoulderTorque = systemTorque - (gravityTorque + inertialTorque); // Instantaneous shoulder torque

                thetaElbow = Vector3.Angle((rightWrist.position - rightElbow.position), (rightShoulder.position - rightElbow.position));
                thetaShoulder = Vector3.Angle((rightElbow.position - rightShoulder.position), GRAVITY_VECTOR);

                armLastCM = armCM;
                lastVelocity = currentVelocity;

                return (double)shoulderTorque.magnitude;
            }

            armLastCM = armCM;
            return 0;
        }

        public double CalculateCorrectionTerm(double angleShoulder)
        {
            double g_beta;
            double t_sin;

            if (gender == "Male")
            {
                g_beta = 1230;
                t_sin = 0.09;
            }
            else
            {
                g_beta = 1005;
                t_sin = 0.11;
            }

            return 0.0095 * g_beta / (1 + Math.Exp((66.40 - angleShoulder) / 7.83)) - Math.Sin(angleShoulder * (2 * Math.PI / 360)) / t_sin;
        }

        public double CalculateRevisedAvgTorque(double rtTorque, double corr, double avgTorque, float delta, double totalTime)
        {
            return ((rtTorque + corr) * delta + avgTorque * (totalTime - delta)) / totalTime;
        }

        public double EstimateChaffinStrength(double angleShoulder, double angleElbow)
        {
            double g_shld;

            if (gender == "Male")
                g_shld = 0.2845;
            else
                g_shld = 0.1495;

            return (227.338 + 0.525 * angleElbow - 0.296 * angleShoulder) * g_shld;
        }

        public double CalculateRevisedMVC(double avgTorque, double chaffinTorque)
        {
            return (avgTorque / chaffinTorque) * 100;
        }

        public double[] EstimateFatiguePrediction(double currentMVC, double totalTime)
        {
            double[] currentIndicator = new double[2];
            double currentET = (14.86 / Math.Pow(currentMVC, 1.83)) / 0.000218;
            double currentFatigue = totalTime / currentET * 100;

            currentIndicator[0] = currentET;
            currentIndicator[1] = currentFatigue;

            return currentIndicator;
        }

        /*public double EstimateFatiguePrediction(double currentMVC, double totalTime)
        {
            return totalTime / ((14.86 / Math.Pow(currentMVC, 1.83)) / 0.000218) * 100;
        }*/

        public double CalculateReducedTorque(double lastChaffin, double lastNicer, float delta, double totalTime)
        {
            return (Math.Pow((totalTime - delta) * 0.000218 / (lastNicer / 100 * 14.86), (-1.0 / 1.83))) / 100 * lastChaffin;
        }

        public double[] EstimateReducedFatigue(double prediction_last, float delta, double totalTime)
        {
            double[] currentIndicator = new double[2];
            double currentFatigue = prediction_last * Math.Exp(-0.04 * delta);
            double currentET = totalTime / (currentFatigue / 100);

            currentIndicator[0] = currentET;
            currentIndicator[1] = currentFatigue;

            return currentIndicator;
        }

        /*public double EstimateReducedFatigue(double prediction_last, float delta)
        {
            return prediction_last * Math.Exp(-0.04 * delta);
        }*/
    }
}
