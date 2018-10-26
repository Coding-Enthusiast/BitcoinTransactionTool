using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BitcoinTransactionTool.Services
{
    public class ErrorCollection : IReadOnlyCollection<string>
    {
        private readonly List<string> ErrorList = new List<string>();


        /// <summary>
        /// Returns true if there is any errors available.
        /// </summary>
        /// <returns>true if there is any error, false if empty.</returns>
        public bool Any()
        {
            return ErrorList.Count > 0;
        }

        /// <summary>
        /// Adds one error to the list of errors
        /// </summary>
        /// <param name="errorMessage">Error string to add.</param>
        internal void Add(string errorMessage)
        {
            ErrorList.Add(errorMessage);
        }

        /// <summary>
        /// Adds multiple errors to the list of errors
        /// </summary>
        /// <param name="multiError">List of errors to add.</param>
        internal void AddRange(IEnumerable<string> multiError)
        {
            ErrorList.AddRange(multiError);
        }

        /// <summary>
        /// Clears all errors
        /// </summary>
        internal void Clear()
        {
            ErrorList.Clear();
        }

        /// <summary>
        /// Returns a formated string of all the errors.
        /// </summary>
        /// <returns>Formatted string of all the errors</returns>
        public string GetErrors()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ErrorList)
            {
                sb.AppendLine("- " + item);
            }
            return sb.ToString();
        }


        #region IReadOnlyCollection

        public int Count
        {
            get { return ErrorList.Count; }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ErrorList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)ErrorList).GetEnumerator();
        }

        #endregion

    }
}
