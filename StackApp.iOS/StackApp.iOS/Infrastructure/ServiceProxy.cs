using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using Stacklash.iOS;

namespace Stacklash.Core
{
    public class ServiceProxy
    {
        private const string _apiKey = "QQRDHskeR71FapXrpWod)g((";
        private const string _clientId = "1750";

        private static string _site;

        private const string questionDetailsFilter = "!LpJqjYARSaWSs4(S0y2cbc";
        private const string questionsListFilter = "!)EczBsxY)eWJ8IsGQLGqecIpW)7.9jUpfh1Ae40-BvLu-tVyk";

        private const string answersListFilter = "!.FgbT-KbkWIdbM1XdnhMvFZldBAlF";

        private const string commentsFilter = "!6LE4lWdui(IRJ";

        private const string usersListFilter = "!T6o*9ZJwHSPyZwcNNg";
        private const string userDetailsFilter = "!9j_cPhPwl";

        private const string sitesListFilter = "!SmO)Jx)5JqkELDzUFR";
        private const string siteInfoFilter = "!9j_cPoVcR";

        private const string badgesFilter = "!9j_cPmBiv";
        private const string notificationsFilter = "!6P.fWIZ1IkFLo";
        private const string inboxFilter = "!*L6XYDQ0pMXwDCYU";

        public static void InitSite(string siteName)
        {
            _site = siteName;
        }

        public static string GetLoginUrl() {
            const string scope = "read_inbox,no_expiry,write_access,private_info";
            const string redirectUrl = "https://stackexchange.com/oauth/login_success";
            string url = string.Format("https://stackexchange.com/oauth/dialog?client_id={0}&scope={1}&redirect_uri={2}", _clientId, scope, redirectUrl);
            return url;
        }

        public static Task<BaseResponse<ShallowUser>> GetModerators(UserSortBy sort, int page) {
            return GetResponse<ShallowUser>("users/moderators?order=desc&filter=" + usersListFilter, page: page, usort: sort);
        }

        public static Task<BaseResponse<ShallowUser>> GetUsers(UserSortBy sort, int page, string inname = null)
        {
            string url = "users?order=desc&filter=" + usersListFilter;
            if (inname != null)
                url += "&inname=" + inname;

            return GetResponse<ShallowUser>(url, page: page, usort: sort);
        }

        public static Task<BaseResponse<User>> GetUserById(int userId)
        {
            return GetResponse<User>(string.Format("users/{0}?order=desc&filter={1}", userId, userDetailsFilter));
        }

        public static Task<BaseResponse<Post>> GetSearchResults(string text) {
            return GetResponse<Post>("search/advanced?q=" + HttpUtility.UrlEncode(text));
        }

        public async static Task<BaseResponse<ShallowUser>> GetUsersByIds(List<int> userIds)
        {
            if (!userIds.Any())
                return new BaseResponse<ShallowUser>();

            var response = await GetResponse<ShallowUser>(string.Format("users/{0}?order=desc&filter={1}", userIds.JoinStrings(";"), usersListFilter));

            var ordered = (from i in userIds 
                          join o in response.items 
                          on i equals o.user_id 
                          select o).ToList();

            response.items = ordered;

            return response;
        }

        public static Task<BaseResponse<User>> GetMe()
        {
            return GetResponse<User>(string.Format("me?filter={0}", userDetailsFilter));
        }

        public static Task<BaseResponse<SiteStatistics>> GetSiteInfo(string site)
        {
            return GetResponse<SiteStatistics>("info?filter=" + siteInfoFilter, site: site);
        }

        public static Task<BaseResponse<Site>> GetAllSites()
        {
            return GetResponse<Site>("sites?pagesize=300&filter=" + sitesListFilter, includeSiteParam: false);
        }

        public static Task<BaseResponse<Badge>> GetUserBadges(int userId, BadgeSortBy sort, int page)
        {
            return GetResponse<Badge>(string.Format("users/{0}/badges?order=desc&filter={1}", userId, badgesFilter), bsort: sort, page: page);
        }

        public static Task<BaseResponse<Tag>> GetAllTagsByPopularity(int page, int count, string inname = null)
        {
            return GetAllTagsImpl(page, count, inname, "popular", isAscending: false);
        }

        public static Task<BaseResponse<Tag>> GetAllTagsByName(int page, int count, string inname = null)
        {
            return GetAllTagsImpl(page, count, inname, "name", isAscending: true);
        }

        private static Task<BaseResponse<Tag>> GetAllTagsImpl(int page, int count, string inname, string sort, bool isAscending)
        {
            string url = "tags?sort=" + sort;
            if (inname != null)
                url += "&inname=" + inname;

            url += "&page=" + page;
            url += "&pagesize=" + count;
            url += "&order=" + (isAscending ? "asc" : "desc");

            return GetResponse<Tag>(url);
        }

        public static Task<BaseResponse<Post>> GetFavoritesForUser(int userId, PostSortBy sort, int page)
        {
            return GetResponse<Post>(string.Format("users/{0}/favorites?order=desc&filter={1}", userId, questionsListFilter), psort: sort, page: page);
        }

        public static Task<BaseResponse<Timeline>> GetUserTimeline(int userId, int page)
        {
            return GetResponse<Timeline>(string.Format("users/{0}/timeline", userId), page: page);
        }

        public static Task<BaseResponse<Comment>> GetQuestionComments(int questionId, PostSortBy sortBy, int page) {
            return GetResponse<Comment>(string.Format("questions/{0}/comments?order=asc&filter={1}", questionId, commentsFilter), psort: sortBy, page: page);
        }

        public static Task<BaseResponse<Comment>> GetAnswerComments(int answerId, PostSortBy sortBy, int page) {
            return GetResponse<Comment>(string.Format("answers/{0}/comments?order=asc&filter={1}", answerId, commentsFilter), psort: sortBy, page: page);
        }

        public static Task<BaseResponse<Comment>> GetUserMentions(int userId, int page) {
            return GetResponse<Comment>(string.Format("users/{0}/mentioned?order=desc&sort=creation&filter={1}", userId, commentsFilter), page: page);
        }

        public static Task<BaseResponse<Post>> GetQuestionsForUser(int userId, PostSortBy sort, int page)
        {
            return GetResponse<Post>(string.Format("users/{0}/questions?order=desc&filter={1}", userId, questionsListFilter), psort: sort, page: page);
        }

        public static Task<BaseResponse<Post>> GetQuestionsByIds(List<int> ids) {
            return GetResponse<Post>(string.Format("questions/{0}?order=desc&filter={1}", ids.JoinStrings(";"), questionsListFilter));
        }

        public static Task<BaseResponse<Post>> GetQuestions(PostFilter postFilter, PostSortBy sort, int page, string tag = null)
        {
            string url = "questions{0}?order=desc&filter={1}";
            if (tag != null)
                url += "&tagged=" + HttpUtility.UrlEncode(tag);

            string postFilterStr = "";
            if (postFilter != PostFilter.normal)
            {
                string val = postFilter.ToString();
                if (postFilter == PostFilter.noanswers)
                    val = "no-answers";

                postFilterStr = "/" + val;
            }

            url = string.Format(url, postFilterStr, questionsListFilter);

            return GetResponse<Post>(url, psort: sort, page: page);
        }

        public static Task<BaseResponse<Notification>> GetNotifications(int page) {
            return GetResponse<Notification>("notifications?filter=" + notificationsFilter, page: page, includeSiteParam:false);
        }

        public static Task<BaseResponse<InboxItem>> GetInbox(int page) {
            return GetResponse<InboxItem>("inbox?filter=" + inboxFilter, page: page, includeSiteParam: false);
        }

        public static Task<BaseResponse<TopTag>> GetTopTagsForUser(int userId, int page) {
            return GetResponse<TopTag>(string.Format("users/{0}/top-answer-tags", userId), page: page);
        }

        public static Task<BaseResponse<Post>> GetTopQuestionsByUserAndTag(int userId, PostSortBy sort, string tag, int page) {
            string url = string.Format("users/{0}/tags/{1}/top-answers?order=desc&filter={2}", userId, HttpUtility.UrlEncode(tag), questionDetailsFilter);
            return GetResponse<Post>(url, page: page, psort: sort);
        }

        public static Task<BaseResponse<Post>> GetPostById(int postId) {
            string url = string.Format("questions/{0}?order=desc&filter={1}", postId, questionDetailsFilter);

            return GetResponse<Post>(url);
        }

        public static Task<BaseResponse<Post>> GetAnswerById(int postId) {
            string url = string.Format("answers/{0}?order=desc&filter={1}", postId, answersListFilter);

            return GetResponse<Post>(url);
        }

        public static Task<BaseResponse<Post>> GetAnswersForQuestion(int questionId, PostSortBy sort, int page)
        {
            return GetResponse<Post>(string.Format("questions/{0}/answers?order=desc&filter={1}", questionId, answersListFilter), psort: sort, page: page);
        }

        public static Task<BaseResponse<Post>> GetAnswersForUser(int userId, PostSortBy sort, int page)
        {
            return GetResponse<Post>(string.Format("users/{0}/answers?order=desc&filter={1}", userId, answersListFilter), psort: sort, page: page);
        }

        private static async Task<BaseResponse<T>> GetResponse<T>(
            string relativeUrl, PostSortBy? psort = null, UserSortBy? usort = null, BadgeSortBy? bsort = null, 
            string site = null, int? page = null, bool includeSiteParam = true) where T : new()
        {
            if (site == null && includeSiteParam)
                site = _site;

            string url = "https://api.stackexchange.com/2.1/" + relativeUrl;

            if (site != null)
                AddParam(ref url, "site", site);

            if (psort.HasValue)
                AddParam(ref url, "sort", psort.Value.ToString());

            if (usort.HasValue)
                AddParam(ref url, "sort", usort.Value.ToString());

            if (bsort.HasValue)
                AddParam(ref url, "sort", bsort.Value.ToString());
            
            if (page.HasValue)
                AddParam(ref url, "page", page.ToString());

            AddParam(ref url, "key", _apiKey);

            string accessToken = Config.AccessToken;
            if (accessToken.IsNotNullOrEmpty())
                AddParam(ref url, "access_token", accessToken);

            Console.WriteLine(url);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.ReadWriteTimeout = 30000;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (!IOS.IsStackApiReachable()) 
                return new BaseResponse<T> { Success = false, ErrorMessage = "No internet connection" };

            try {
                using (var stream = (await request.GetResponseAsync().ConfigureAwait(false)).GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();

                    var data = JsonConvert.DeserializeObject<BaseResponse<T>>(text);
                    data.Success = true;
                    return data;
                }
            }
            catch (WebException ex) {
                return new BaseResponse<T> { Success = false, ErrorMessage = ex.Message };
            }
        }

        private static void AddParam(ref string url, string name, string value)
        {
            if (!url.Contains('?'))
                url += "?" + name + "=" + value;
            else
                url += "&" + name + "=" + value;
        }
    }

    public enum PostFilter
    {
        normal,
        unanswered,
        noanswers,
        featured
    }

    public enum PostSortBy
    {
        activity,
        creation,
        votes,
        hot,
        week,
        month
    }

    public enum UserSortBy
    {
        reputation,
        creation,
        name,
        modified
    }

    public enum BadgeSortBy
    {
        rank,
        name,
        type,
        awarded
    }

    public class ShallowUser
    {
        public int user_id { get; set; }
        public string display_name { get; set; }
        public int reputation { get; set; }
        public string user_type { get; set; }
        public string profile_image { get; set; }
        public string link { get; set; }
        public int accept_rate { get; set; }

        public string NameDecoded { get { 
                if (string.IsNullOrEmpty(display_name))
                    return "";
                else 
                    return HttpUtility.HtmlDecode(display_name); 
            } }
    }

    public class User : ShallowUser
    {
        public int creation_date { get; set; }
        public int reputation_change_day { get; set; }
        public int reputation_change_week { get; set; }
        public int reputation_change_month { get; set; }
        public int reputation_change_quarter { get; set; }
        public int reputation_change_year { get; set; }
        public int age { get; set; }
        public int last_access_date { get; set; }
        public int last_modified_date { get; set; }
        public bool is_employee { get; set; }
        public string website_url { get; set; }
        public string location { get; set; }
        public int account_id { get; set; }
        public BadgeCounts badge_counts { get; set; }
        public int question_count { get; set; }
        public int answer_count { get; set; }
        public int up_vote_count { get; set; }
        public int down_vote_count { get; set; }
        public string about_me { get; set; }
        public int view_count { get; set; }
    }

    public class BadgeCounts
    {
        public int gold { get; set; }
        public int silver { get; set; }
        public int bronze { get; set; }
    }

    public class Post
    {
        public string body { get; set; }
        public int question_id { get; set; }
        public int answer_id { get; set; }
        public int creation_date { get; set; }
        public int last_activity_date { get; set; }
        public int score { get; set; }
        public bool is_accepted { get; set; }
        public ShallowUser owner { get; set; }
        public int? last_edit_date { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public int answer_count { get; set; }
        public int view_count { get; set; }
        public int favorite_count { get; set; }
        public bool is_answered { get; set; }
        public string link { get; set; }
        public List<Comment> comments { get; set; }

        public string TitleDecoded {
            get {
                if (title == null)
                    return "";

                return HttpUtility.HtmlDecode(title);
            }
        } 

        public string BodyDecoded {
            get {
                if (body == null)
                    return "";

                return HttpUtility.HtmlDecode(body);
            }
        } 
    }

    public class Site
    {
        public string site_type { get; set; }
        public string name { get; set; }
        public string logo_url { get; set; }
        public string api_site_parameter { get; set; }
        public string site_url { get; set; }
        public string audience { get; set; }
        public string icon_url { get; set; }
        public List<string> aliases { get; set; }
        public string site_state { get; set; }
        public int launch_date { get; set; }
        public string favicon_url { get; set; }
        public List<string> markdown_extensions { get; set; }
        public string high_resolution_icon_url { get; set; }

        public string NameDecoded {
            get {
                if (name == null)
                    return "";

                return HttpUtility.HtmlDecode(name);
            }
        } 

        public string AudienceDecoded {
            get {
                if (audience == null)
                    return "";

                return HttpUtility.HtmlDecode(audience);
            }
        } 
    }

    public class SiteStatistics
    {
        public int total_questions { get; set; }
        public int total_unanswered { get; set; }
        public int total_accepted { get; set; }
        public int total_answers { get; set; }
        public double questions_per_minute { get; set; }
        public double answers_per_minute { get; set; }
        public int total_comments { get; set; }
        public int total_votes { get; set; }
        public int total_badges { get; set; }
        public double badges_per_minute { get; set; }
        public int total_users { get; set; }
        public int new_active_users { get; set; }
        public string api_revision { get; set; }
        public Site site { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public int count { get; set; }
        public bool is_required { get; set; }
        public bool is_moderator_only { get; set; }
        public bool has_synonyms { get; set; }
    }

    public class Timeline
    {
        public int creation_date { get; set; }
        public string post_type { get; set; }
        public string timeline_type { get; set; }
        public int user_id { get; set; }
        public int post_id { get; set; }
        public int comment_id { get; set; }
        public string title { get; set; }
        public string detail { get; set; }

        public string DetailDecoded {
            get {
                if (detail == null)
                    return "";

                return HttpUtility.HtmlDecode(detail);
            }
        }

        public string TitleDecoded {
            get {
                if (title == null)
                    return "";

                return HttpUtility.HtmlDecode(title);
            }
        }
    }

    public class Badge
    {
        public int badge_id { get; set; }
        public string rank { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int award_count { get; set; }
        public string badge_type { get; set; }
        public User user { get; set; }
        public string link { get; set; }
    }

    public class TopTag
    {
        public string tag_name { get; set; }
        public int question_score { get; set; }
        public int question_count { get; set; }
        public int answer_score { get; set; }
        public int answer_count { get; set; }
        public int user_id { get; set; }
    }

    public class Comment
    {
        public int comment_id { get; set; }
        public int creation_date { get; set; }
        public int score { get; set; }
        public bool edited { get; set; }
        public string body { get; set; }
        public ShallowUser owner { get; set; }
        public string link { get; set; }
        public ShallowUser reply_to_user { get; set; }

        public string BodyDecoded {
            get {
                if (body == null)
                    return "";

                return HttpUtility.HtmlDecode(body);
            }
        }
    }

    public class Notification
    {
        public string notification_type { get; set; }
        public Site site { get; set; }
        public int creation_date { get; set; }
        public string body { get; set; }
        public bool is_unread { get; set; }
        public int? post_id { get; set; }
    }

    public class InboxItem
    {
        public string item_type { get; set; }
        public int question_id { get; set; }
        public int comment_id { get; set; }
        public string title { get; set; }
        public int creation_date { get; set; }
        public bool is_unread { get; set; }
        public Site site { get; set; }
        public string body { get; set; }
        public string link { get; set; }
        public int? answer_id { get; set; }

        public string TitleDecoded {
            get {
                if (title == null)
                    return "";

                return HttpUtility.HtmlDecode(title);
            }
        } 

        public string BodyDecoded {
            get {
                if (body == null)
                    return "";

                return HttpUtility.HtmlDecode(body);
            }
        } 
    }

    public class BaseResponse<T>  {
        public List<T> items { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int quota_remaining { get; set; }
        public int quota_max { get; set; }
        public bool has_more { get; set; }
    }
}