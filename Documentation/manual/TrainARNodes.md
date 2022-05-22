# TrainAR Nodes

TrainAR Nodes are used in the TrainAR Stateflow to model the behaviour and procedural flow of the AR trainings created with TrainAR. TrainAR Nodes define what the next correct step in your training is but also how your training responds to correct or incorrect actions taken by the user. Consequently, there are TrainAR Nodes which **wait** for an **Action** to be taken by the user, these are called **TrainAR Actions**. **TrainAR Actions** consist of actions taken in the AR-context (so, grabbing, interacting or combining) or UI-actions (i.e a UI element which pops up and asks the user a question). How your training reacts to correct or incorrect steps taken by the user are defined by **TrainAR Insight-, Instruction-, Feedback- and Object Helper Nodes**. They are triggered instantly and continue the stateflow automatically after they performed their task. With these, you can instruct the user of the training, define the consequences of the users actions in the AR-context and give feedback or expert insights for these actions in the form of textual information displayed on the UI.

## Overview

| TrainAR Node | Result | Description |
| :----------------------: |:-------------------------:| :-------------------:|
|![](../resources/StartEvent.png)|<img src="../resources/TrainAR_Training_Onboarding.png" alt="drawing" width="300"/>|Starts the TrainAR Stateflow<br/><br/>[Read more](StartTrainingNode.html)|
|![](../resources/Action.PNG)|<img src="../resources/TrainAR_Training_Action.png" alt="drawing" width="250"/>|Waits for a Action (Interact, Combine or Custom)<br/><br/>[Read more](ActionNodes.html)|
|![](../resources/ActionFork.PNG)|<img src="../resources/TrainAR_Training_Action2.png" alt="drawing" width="250"/>|Waits for an Action, Forks the Stateflow based on which Action was performed<br/><br/>[Read more](ActionNodes.html)|
|![](../resources/ActionMulti_2.PNG)|<img src="../resources/TrainAR_Training_Action3.png" alt="drawing" width="250"/>|Waits for _n_ Actions to be performed (in any order)<br/><br/>[Read more](ActionNodes.html)|
|![](../resources/ActionUI_Questionnaire.PNG)|<img src="../resources/TrainAR_Training_Questionnaire.png" alt="drawing" width="250"/>|Triggers and waits for a UI quiz to be answered<br/><br/>[Read more](UIActionNodes.html)|
|![](../resources/instructions.PNG)|<img src="../resources/TrainAR_Training_Instructions.png" alt="drawing" width="250"/>|Shows new instructions to the user<br/><br/>[Read more](InstructionNode.html)|
|![](../resources/Feedback.png)|<img src="../resources/TrainAR_Training_Feedback.png" alt="drawing" width="250"/>|Shows a feedback overlay to the user<br/><br/>[Read more](FeedbackNode.html)|
|![](../resources/Insights.PNG)|<img src="../resources/TrainAR_Training_Insights.png" alt="drawing" width="250"/>|Shows Insights (e.g. additional tips and tricks) to the user<br/><br/>[Read more](InsightNode.html)|
|![](../resources/ObjectHelper.png)|<img src="../resources/TrainAR_Training_ObjectHelper.png" alt="drawing" width="250"/>|Performs object-level behaviour/state changes<br/><br/>[Read more](ObjectHelperNode.html)|
|![](../resources/TrainingConclusion.PNG)|<img src="../resources/TrainAR_Training_TrainingAssessment.png" alt="drawing" width="250"/>|Concludes the training and TrainAR Stateflow<br/><br/>[Read more](TrainingConclusionNode.html)|
