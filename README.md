# NICER_Unity_API
 
NICER is a New and Improved Consumed Endurance and Recovery metric to quantify muscle fatigue of upper-body mid-air interactions. Using joint positions of the performing arm, the real-time shoulder fatigue levels are estimated to provide an insightful understanding of the user interaction experience.

Features
Predicts the maximum interaction duration based on
The user’s biological sex: Male/Female
The continuously performed arm gestures
The (natural) pause taken during the interaction
Estimates the break duration based on the desired decrease by interaction designers in fatigue levels.
Works with interactions of varying physical intensities.
Works with different modalities, from barehand interactions to controller-based interactions.
Reliable performance in above-shoulder interactions based on a novel hybrid exertion estimation.
Gesture-based maximum strength estimated from dynamic arm movement.

How To Use
Put the plug-in Unity library NICER.dll in the working Unity Project.
Assign FatigueEstimation.cs to any chosen object in the working scene.
Add tracking sources of joints Hand, Wrist, Elbow, and Shoulder to the public fields of FatigueEstimation.cs.
Run the scene.

Tracking Source of Arm Posture
Inverse Kinematic approach: [VRArmIK](https://github.com/dabeschte/VRArmIK?tab=readme-ov-file) + [OVRSkeleton](https://developer.oculus.com/reference/unity/v64/class_o_v_r_skeleton/) 
Computer Vision approach:  [VNect](https://dl.acm.org/doi/abs/10.1145/3072959.3073596?casa_token=XTtT2sIzTUUAAAAA:7tISEOf7lO3jIVOHM54kAtmcksxW7IenhQblQ2Ewnf3LdRqKKqRHQcSrRJ95ToYGaE_PrawhWUB_8w)
Motion Capturing sensors: [Kinect V2 camera](https://learn.microsoft.com/en-us/windows/apps/design/devices/kinect-for-windows), [Vicon Nexus](https://www.vicon.com/software/nexus/), [Inertial Measurement Units (IMU)](https://www.movella.com/products/wearables/xsens-mtw-awinda)

Reimplement NICER in other languages
The current implementation is written in C#, reimplementations in other languages are more than welcome.

If you have any other questions, please open an issue in the issue tracker or contact yi.li5@monash.edu.
