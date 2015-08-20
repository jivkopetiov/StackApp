using NUnit.Framework;
using StackApp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackApp.Cmd
{
    [TestFixture]
    public class ServiceProxyTests
    {
        [SetUp]
        public void Init()
        {
            ServiceProxy.InitSite("stackoverflow");
        }

        [Test]
        public void GetTags()
        {
            var tags = ServiceProxy.GetAllTagsByPopularity(1, "man").Result;
            Assert.IsNotEmpty(tags.items);
        }

        [Test]
        public void GetQuestions()
        {
            var questions = ServiceProxy.GetQuestions(PostFilter.normal, PostSortBy.votes, 1).Result;
            Assert.IsNotEmpty(questions.items);
        }

        [Test]
        public void GetQuestionsTagged()
        {
            var questions = ServiceProxy.GetQuestions(PostFilter.normal, PostSortBy.creation, 1, tag: "monotouch").Result;
            Assert.IsNotEmpty(questions.items);
        }

        [Test]
        public void GetQuestionsForUser()
        {
            var questions = ServiceProxy.GetQuestionsForUser(22656, PostSortBy.votes, 1).Result;
            Assert.IsNotEmpty(questions.items);
        }

        [Test]
        public void GetBadgesForUser()
        {
            var badges = ServiceProxy.GetUserBadges(22656, BadgeSortBy.type, 1).Result;
            Assert.IsNotEmpty(badges.items);
        }

        [Test]
        public void GetTimelineForUser()
        {
            var timeline = ServiceProxy.GetUserTimeline(22656, 1).Result;
            Assert.IsNotEmpty(timeline.items);
        }

        [Test]
        public void GetFavoritesForUser()
        {
            var questions = ServiceProxy.GetFavoritesForUser(22656, PostSortBy.votes, 1).Result;
            Assert.IsNotEmpty(questions.items);
        }

        [Test]
        public void GetAnswersForQuestion()
        {
            var answers = ServiceProxy.GetAnswersForQuestion(11227809, PostSortBy.creation, 1).Result;
            Assert.IsNotEmpty(answers.items);
        }

        [Test]
        public void GetAnswersForUser()
        {
            var answers = ServiceProxy.GetAnswersForUser(22656, PostSortBy.votes, 1).Result;
            Assert.IsNotEmpty(answers.items);
        }

        [Test]
        public void GetUsers()
        {
            var users = ServiceProxy.GetUsers(UserSortBy.reputation, 1).Result;
            Assert.IsNotEmpty(users.items);
        }

        [Test]
        public void GetUserById()
        {
            var user = ServiceProxy.GetUserById(22656).Result;
            Assert.IsNotNull(user);
        }

        [Test]
        public void GetAllSites()
        {
            var sites = ServiceProxy.GetAllSites().Result;
            Assert.IsNotEmpty(sites.items);
        }

        [Test]
        public void GetSiteInfo()
        {
            var sites = ServiceProxy.GetSiteInfo("serverfault").Result;
            Assert.IsNotEmpty(sites.items);
        }
    }
}












