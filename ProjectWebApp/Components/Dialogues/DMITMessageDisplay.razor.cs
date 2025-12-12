using Microsoft.AspNetCore.Components;

namespace ProjectWebApp.Components.Dialogues
{
    public partial class DMITMessageDisplay
    {
        #region Parameters
        // A list of error messages that can be displayed as a collection.
        [Parameter]
        public List<string> ErrorMsgs { get; set; } = [];

        // A single error message to be displayed if there are no multiple errors.
        [Parameter]
        public string ErrorMessage { get; set; } = string.Empty;

        // A general feedback message to show success or info messages to the user.
        [Parameter]
        public string Feedback { get; set; } = string.Empty;
       #endregion

        #region Fields
        // Returns true if Feedback has any non-whitespace content.
        private bool hasFeedback => !string.IsNullOrWhiteSpace(Feedback);

        // Returns true if a single ErrorMessage has content.
        private bool hasSingleError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // Returns true if there are multiple error messages to display.
        private bool hasMultipleErrors => ErrorMsgs.Count > 0;
        #endregion
    }
}
