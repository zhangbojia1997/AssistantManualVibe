using System;
using RSP.Common;

namespace RSP.AgileAssistant.Business.Base.Bo
{
    /// <summary>
    /// Standard result envelope returned by every Action. Wraps the operation
    /// outcome, the produced data, and any error details.
    /// </summary>
    /// <typeparam name="T">Type of the data payload produced by the Action.</typeparam>
    public class BusinessResult<T>
    {
        /// <summary>
        /// Initializes a successful or failed result with the supplied values.
        /// </summary>
        private BusinessResult(bool isSuccess, T? data = default, string errorMessage = "", Exception? exception = null)
        {
            this.IsSuccess = isSuccess;
            this.Data = data;
            this.ErrorMessage = errorMessage;
            this.Exception = exception;
        }

        /// <summary>
        /// Indicates whether the Action completed successfully.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Data payload produced by the Action when successful. Hidden from log
        /// serialization and unit-test result comparison; still returned to API
        /// clients.
        /// </summary>
        [Hiding]
        public T? Data { get; private set; }

        /// <summary>
        /// User-friendly error message when the Action failed. Hidden from log
        /// serialization and unit-test result comparison.
        /// </summary>
        [Hiding]
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Captured exception when the Action failed unexpectedly. Hidden from
        /// API serialization to avoid leaking internal details to clients.
        /// </summary>
        [Hiding]
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Creates a successful result carrying the supplied data.
        /// </summary>
        /// <param name="data">The data payload to return.</param>
        public static BusinessResult<T> Success(T data)
        {
            return new BusinessResult<T>(true, data);
        }

        /// <summary>
        /// Creates a failed result with no additional information.
        /// </summary>
        public static BusinessResult<T> Fail()
        {
            return new BusinessResult<T>(false);
        }

        /// <summary>
        /// Creates a failed result with a user-friendly error message.
        /// </summary>
        /// <param name="errorMessage">The user-friendly error message.</param>
        public static BusinessResult<T> Fail(string errorMessage)
        {
            return new BusinessResult<T>(false, default, errorMessage);
        }

        /// <summary>
        /// Creates a failed result with a message and the captured exception.
        /// </summary>
        /// <param name="errorMessage">The user-friendly error message.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        public static BusinessResult<T> Fail(string errorMessage, Exception exception)
        {
            return new BusinessResult<T>(false, default, errorMessage, exception);
        }
    }
}
