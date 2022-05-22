using UnityEngine;

namespace Static
{
    /// <summary>
    /// Custom actions are actions triggered by events outside of Interacting or combining (e.g. UI quizes) and can send parameters
    /// that are then checked against the statemachine.
    /// 
    /// This class is also what authors can use to trigger actions beyond the scope of TrainAR and can be handled by the statemachine
    /// by using the Action Node and the "Custom Action" type.
    /// 
    /// CustomAction can either be triggered form the instantiated or static context.
    /// </summary>
    public class CustomAction : MonoBehaviour
    {
        /// <summary>
        /// Triggers a CustomAction with the given parameter.
        /// </summary>
        /// <param name="parameter">The parameter to be checked against the statemachine.</param>
        public void DynamicTrigger(string parameter)
        {
            StaticTrigger(parameter);
        }
    
        /// <summary>
        /// Triggers a CustomAction with the given parameter from a static context.
        /// </summary>
        /// <param name="parameter">The parameter to be checked against the statemachine.</param>
        /// <returns>Whether this was a correct statechange and triggered to statemachine to proceed.</returns>
        public static bool StaticTrigger(string parameter)
        {
            return StatemachineConnector.Instance.RequestStateChange(new StateInformation(interactionType:InteractionType.Custom, parameter:parameter));
        }
    }
}
