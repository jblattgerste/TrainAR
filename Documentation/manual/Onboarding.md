# Onboarding

Before the stateflow of a training starts, TrainAR takes the trainee through the same steps in every training: choosing the training, an introduction to it, a short onboarding on the interaction concept, and placing the training in the surroundings. Authors do not have to build any of this. It is part of the framework and uses the name, description, expert and preview of the training (see [Managing Trainings](Trainings.md)). When the trainee has placed the training, the [Start Training Node](StartTrainingNode.md) starts the stateflow.

## 1. Home screen and introduction

<div class="trainar-phones">

| Home screen | Introduction of the training |
| :---: | :---: |
| <img src="../resources/Onboarding_Home.png" alt="The home screen with the Coffee and Four-Stroke Engine trainings" width="260"/> | <img src="../resources/Onboarding_Start.png" alt="The introduction of the Coffee training: the expert presents the description, below it the preview of the setup" width="260"/> |
| The trainee chooses a training. | The expert introduces the training with its description; below is the preview of its setup, rendered automatically before every build. |

</div>

## 2. The interaction concept

Five short pages explain how trainees interact with TrainAR trainings, each with an animation, and every page is read aloud. Trainees can skip them on the first of these pages.

<div class="trainar-phones">

| Instructions at the top | Interact | Grab |
| :---: | :---: | :---: |
| <img src="../resources/Onboarding_Page_Instructions.webp" alt="Onboarding page: instructions and progress in the blue bar at the top" width="240"/> | <img src="../resources/Onboarding_Page_Interact.webp" alt="Onboarding page: point at an object, move closer and press Interact" width="240"/> | <img src="../resources/Onboarding_Page_Grab.webp" alt="Onboarding page: grab an object and release it again" width="240"/> |
| The blue bar at the top tells the trainee the next action and shows the progress. A sound tells whether an action was correct. | Point the cursor at an object and move the phone close until it is outlined in blue, then press **Interact**. | The same, then press **Grab**: the object moves with the phone until **Release** is pressed. |

| Combine | Insights and feedback |
| :---: | :---: |
| <img src="../resources/Onboarding_Page_Combine.webp" alt="Onboarding page: move a held object into another one and press Combine" width="240"/> | <img src="../resources/Onboarding_Page_Insights.webp" alt="Onboarding page: an insight of the expert and the feedback overlay" width="240"/> |
| Move a held object into another one: if they can be combined, it turns transparent and **Combine** appears. | The expert shares insights at the top, and for some incorrect actions an overlay explains what went wrong until **Understood** is tapped. |

</div>

## 3. Scanning and placing

<div class="trainar-phones">

| Scanning | Placing |
| :---: | :---: |
| <img src="../resources/Onboarding_Scan.png" alt="Scanning: an animation shows how to move the phone over the table" width="260"/> | <img src="../resources/Onboarding_Place.png" alt="Placing: a preview of the setup follows the phone, the Place button places it" width="260"/> |
| The trainee moves the phone over a table until a surface is detected. | A see-through preview of the setup follows the phone; **Place** puts it down. Now the stateflow of the training starts. |

</div>

## 4. During the training: tracking warnings

If the phone loses track of its surroundings during the training, TrainAR dims the screen and shows what the trainee can do about it. This happens automatically, and the training continues as soon as tracking works again.

<div class="trainar-phones">

| Too dark | Too few features | Too much motion |
| :---: | :---: | :---: |
| <img src="../resources/Tracking_Light.png" alt="Tracking warning: ensure sufficient lighting" width="240"/> | <img src="../resources/Tracking_Features.png" alt="Tracking warning: try a more textured surface" width="240"/> | <img src="../resources/Tracking_Motion.png" alt="Tracking warning: avoid excessive motion" width="240"/> |

</div>
