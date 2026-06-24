using RSP.AgileAssistant.Business.User.Bo;
using RSP.AgileAssistant.Business.User.Vo;

namespace RSP.AgileAssistant.Business.User
{
    /// <summary>
    /// Pure mapping helpers between user business objects and value objects.
    /// </summary>
    internal static class UserMapper
    {
        /// <summary>
        /// Projects a <see cref="UserBo"/> onto a <see cref="UserVo"/>.
        /// </summary>
        /// <param name="bo">Source business object.</param>
        /// <param name="accessToken">Access token to expose, when applicable.</param>
        internal static UserVo ToVo(UserBo bo, string accessToken = "")
        {
            return new UserVo
            {
                Id = bo.Id.ToString(),
                UserName = bo.UserName,
                FullName = bo.FullName,
                DefaultDeckId = bo.DefaultDeckId,
                DefaultGroup = bo.DefaultGroup,
                JiraEmail = bo.JiraEmail,
                HasJiraCredentials = !string.IsNullOrWhiteSpace(bo.JiraEmail) && !string.IsNullOrWhiteSpace(bo.JiraToken),
                UseDefaults = bo.UseDefaults,
                ScrumMaster = bo.ScrumMaster,
                IsGuest = bo.IsGuest,
                AccessToken = accessToken,
            };
        }
    }
}
