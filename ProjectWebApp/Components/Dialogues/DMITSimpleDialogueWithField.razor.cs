using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ProjectWebApp.Components.Dialogues
{
    public partial class DMITSimpleDialogueWithField
    {
        #region Fields
        // This field stores user input from the text field inside the dialogue.
        private string feedbackText = string.Empty;
        #endregion

        #region Parameters
        // This is a special parameter that allows the component to interact with the dialog instance.
        // It is used to close the dialog and return values back to the caller.
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        // This parameter allows the parent component to set the text on the button in the dialog.
        [Parameter]
        public string ButtonText { get; set; } = string.Empty;

        // This parameter lets the parent component specify the color of the button.
        [Parameter]
        public Color Color { get; set; } = Color.Primary;
        #endregion

        #region Methods
        // This method is called when the user confirms or submits the dialog.
        // It closes the dialog and passes the user's input (feedbackText) back to the caller.
        private void Submit() => MudDialog.Close(DialogResult.Ok(feedbackText));

        // This method is called when the user cancels the dialog.
        // It simply closes the dialog without returning any data.
        private void Cancel() => MudDialog.Cancel();
        #endregion
    }
}
