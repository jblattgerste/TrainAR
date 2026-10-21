# Onboarding

Before the stateflow of a training starts, TrainAR takes the trainee through the same steps in every training, from choosing the training to placing it in the surroundings. Authors do not have to build any of this. It is part of the framework and uses the name, description, expert and preview of the training (see [Managing Trainings](Trainings.md)).

## The flow

<div class="trainar-flow">

| Step | What happens |
| :---: | :--- |
| <img src="../resources/Onboarding_Home.png" alt="The home screen with the Coffee and Four-Stroke Engine trainings" width="220"/> | **1. Home screen.** After starting the app, the trainee sees all trainings of the project with their name, description, expert and preview, and chooses one. |
| <img src="../resources/Onboarding_Start.png" alt="The introduction of the Coffee training" width="220"/> | **2. Introduction.** The expert introduces the training with its description. Below is the preview of the training's setup, which TrainAR renders automatically before every build. **Exit** leads back to the home screen. |
| <img src="../resources/Onboarding_Page_Instructions.webp" alt="Onboarding page: instructions and progress in the blue bar at the top" width="220"/> | **3. Instructions at the top.** Five short pages now explain the interaction concept, each with an animation, and every page is read aloud. The first one shows that the blue bar at the top tells the trainee the next action and shows the progress, and that a sound tells whether an action was correct. Trainees can skip the onboarding here. |
| <img src="../resources/Onboarding_Page_Interact.webp" alt="Onboarding page: point at an object, move closer and press Interact" width="220"/> | **4. Interact.** Point the cursor in the middle of the screen at an object and move the phone close until the object is outlined in blue, then press **Interact**. |
| <img src="../resources/Onboarding_Page_Grab.webp" alt="Onboarding page: grab an object and release it again" width="220"/> | **5. Grab.** The same, then press **Grab**: the object moves with the phone until **Release** is pressed. |
| <img src="../resources/Onboarding_Page_Combine.webp" alt="Onboarding page: move a held object into another one and press Combine" width="220"/> | **6. Combine.** Move a held object into another one: if they can be combined, the held object turns transparent and **Combine** appears. |
| <img src="../resources/Onboarding_Page_Insights.webp" alt="Onboarding page: an insight of the expert and the feedback overlay" width="220"/> | **7. Insights and feedback.** The expert shares insights at the top, and for some incorrect actions an overlay explains what went wrong until **Understood** is tapped. **Start** begins the training. |
| <img src="../resources/Onboarding_Scan.png" alt="Scanning: an animation shows how to move the phone over the table" width="220"/> | **8. Scanning.** The camera starts, and an animation shows the trainee how to move the phone over a table until a surface is detected. |
| <img src="../resources/Onboarding_Place.png" alt="Placing: a preview of the setup follows the phone, the Place button places it" width="220"/> | **9. Placing.** A see-through preview of the training's setup follows the phone. **Place** puts it down, and the stateflow of the training starts with the [Start Training Node](StartTrainingNode.md). |

</div>

## During the training: tracking warnings

If the phone loses track of its surroundings during the training, TrainAR dims the screen and shows what the trainee can do about it. This happens automatically, and the training continues as soon as tracking works again.

<div class="trainar-phones">

| Too dark | Too few features | Too much motion |
| :---: | :---: | :---: |
| <img src="../resources/Tracking_Light.png" alt="Tracking warning: ensure sufficient lighting" width="220"/> | <img src="../resources/Tracking_Features.png" alt="Tracking warning: try a more textured surface" width="220"/> | <img src="../resources/Tracking_Motion.png" alt="Tracking warning: avoid excessive motion" width="220"/> |

</div>
