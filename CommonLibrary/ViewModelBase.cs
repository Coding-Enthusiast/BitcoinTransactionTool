namespace CommonLibrary
{
    public class ViewModelBase : InpcBase
    {
        /// <summary>
        /// Used for changing the visibility of error message TextBox.
        /// </summary>
        public bool IsErrorMsgVisible
        {
            get { return isErrorMsgVisible; }
            private set { SetField(ref isErrorMsgVisible, value); }
        }
        private bool isErrorMsgVisible;

        /// <summary>
        /// String containing all the errors.
        /// </summary>
        public string Errors
        {
            get { return errors; }
            set
            {
                if (SetField(ref errors, value))
                {
                    IsErrorMsgVisible = !string.IsNullOrEmpty(value);
                }
            }
        }
        private string errors;

        /// <summary>
        /// Status, showing current action being performed.
        /// </summary>
        public string Status
        {
            get { return status; }
            set { SetField(ref status, value); }
        }
        private string status;

    }
}
