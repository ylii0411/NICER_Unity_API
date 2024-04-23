# NICER_Unity_API
 
NICER is a New and Improved Consumed Endurance and Recovery metric to quantify muscle fatigue of upper-body mid-air interactions. Using joint positions of the performing arm, the real-time shoulder fatigue levels are estimated to provide an insightful understanding of the user interaction experience.

## Features
* Predicts the maximum interaction duration based on
  * The user’s biological sex: Male/Female
  * The continuously performed arm gestures
  * The (natural) pause taken during the interaction
* Estimates the break duration based on the desired decrease by interaction designers in fatigue levels.
* Works with interactions of varying physical intensities.
* Works with different modalities, from barehand interactions to controller-based interactions.
* Reliable performance in above-shoulder interactions based on a novel hybrid exertion estimation.
* Gesture-based maximum strength estimated from dynamic arm movement.

## How To Use
1. Download the plug-in Unity library NICER_Unity_API.dll in the working Unity Project.
2. Assign NICER_API.cs to any chosen object in the working scene.
3. Add tracking sources of joints Hand, Wrist, Elbow, and Shoulder to the public fields of FatigueEstimation.cs.
4. Run the scene.

## Tracking Source of Arm Posture
* Inverse Kinematic approach: [VRArmIK](https://github.com/dabeschte/VRArmIK?tab=readme-ov-file) + [OVRSkeleton](https://developer.oculus.com/reference/unity/v64/class_o_v_r_skeleton/) 
* Computer Vision approach:  [VNect](https://dl.acm.org/doi/abs/10.1145/3072959.3073596?casa_token=XTtT2sIzTUUAAAAA:7tISEOf7lO3jIVOHM54kAtmcksxW7IenhQblQ2Ewnf3LdRqKKqRHQcSrRJ95ToYGaE_PrawhWUB_8w)
* Motion Capturing sensors: [Kinect V2 camera](https://learn.microsoft.com/en-us/windows/apps/design/devices/kinect-for-windows), [Vicon Nexus](https://www.vicon.com/software/nexus/), [Inertial Measurement Units (IMU)](https://www.movella.com/products/wearables/xsens-mtw-awinda)

## Reimplement NICER in other languages
The current implementation is written in C#, reimplementations in other languages are more than welcome.

If you have any other questions, please open an issue in the issue tracker or contact yili34@acm.org.

## Acknowledgement
Please cite the following publication when you intend to use it for your research:

[1] Yi Li, Benjamin Tag, Shaozhang Dai, Robert Crowther, Tim Dwyer, Pourang
Irani, and Barrett Ens. 2024. NICER: A New and Improved Consumed En-
durance and Recovery Metric to Quantify Muscle Fatigue of Mid-Air Inter-
actions. ACM Trans. Graph. 43, 4 (July 2024), 14 pages. https://doi.org/10.1145/3658230

[2] Hincapié-Ramos, Juan David, Xiang Guo, Paymahn Moghadasian, and Pourang Irani. 2014. Consumed endurance: a metric to quantify arm fatigue of mid-air interactions. In Proceedings of the SIGCHI Conference on Human Factors in Computing Systems, pp. 1063-1072. April 2014. https://dl.acm.org/doi/10.1145/2556288.2557130
