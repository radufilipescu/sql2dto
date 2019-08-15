using sql2dto.Attributes;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.FetchOpTests
{

    public class MixedDataRowsTests
    {
        [ColumnsPrefix("Blog")]
        public class Blog
        {
            [PropMap(IsKey = true)]
            public int Id { get; set; }

            public string Author { get; set; }

            public List<Post> Posts { get; set; } = new List<Post>();
        }

        [ColumnsPrefix("Post")]
        public class Post
        {
            [PropMap(IsKey = true)]
            public int Id { get; set; }

            public string Title { get; set; }

            public List<Comment> Comments { get; set; } = new List<Comment>();
        }

        [ColumnsPrefix("Comment")]
        public class Comment
        {
            [PropMap(IsKey = true)]
            public int Id { get; set; }
            public string Content { get; set; }
        }

        [Fact]
        public void Mixed_parent_rows_with_children()
        {
            var fakeReader = new FakeDataReader(
                ("BlogId", typeof(int)),    ("BlogAuthor", typeof(string)),
                ("PostId", typeof(int)),    ("PostTitle", typeof(string)),
                ("CommentId", typeof(int)), ("CommentContent", typeof(string))
            );

            fakeReader.AddRow(1, "Radu",    1, "Radu-First",    null, null);
            fakeReader.AddRow(1, "Radu",    2, "Radu-Second",   1, "Radu-Second-C1");
            fakeReader.AddRow(1, "Radu",    2, "Radu-Second",   2, "Radu-Second-C2");
            fakeReader.AddRow(2, "Bob",     3, "Bob-First",     null, null);
            fakeReader.AddRow(1, "Radu",    2, "Radu-Second",   3, "Radu-Second-C3");
            fakeReader.AddRow(1, "Radu",    4, "Radu-Third",    null, null);
            fakeReader.AddRow(1, "Radu",    5, "Radu-Fourth",   null, null);
            fakeReader.AddRow(2, "Bob",     6, "Bob-Second",    null, null);
            fakeReader.AddRow(2, "Bob",     7, "Bob-Third",     null, null);
            fakeReader.AddRow(1, "Radu",    8, "Radu-Fifth",    null, null);
            fakeReader.AddRow(1, "Radu",    2, "Radu-Second",   4, "Radu-Second-C4");
            fakeReader.AddRow(1, "Radu",    5, "Radu-Fourth",   null, null);
            fakeReader.AddRow(2, "Bob",     7, "Bob-Third",     5, "Bob-Third-C1");

            var h = new ReadHelper(fakeReader);

            var fetch =
                h.Fetch<Blog>()
                    .Include<Post>((blog, post) => blog.Posts.Add(post), then: (_) => _
                        .Include<Comment>((post, comment) => post.Comments.Add(comment)
                    ));

            var result = fetch.All();

            // BLOG CHECKS
            Assert.True(result.Count == 2);
            Assert.True(result[0].Author == "Radu");
            Assert.True(result[1].Author == "Bob");

            Assert.True(result[0].Posts.Count == 5);
            Assert.True(result[1].Posts.Count == 3);

            // POST CHECKS
            Assert.True(result[0].Posts[0].Title == "Radu-First");
            Assert.True(result[0].Posts[1].Title == "Radu-Second");
            Assert.True(result[0].Posts[2].Title == "Radu-Third");
            Assert.True(result[0].Posts[3].Title == "Radu-Fourth");
            Assert.True(result[0].Posts[4].Title == "Radu-Fifth");

            Assert.True(result[1].Posts[0].Title == "Bob-First");
            Assert.True(result[1].Posts[1].Title == "Bob-Second");
            Assert.True(result[1].Posts[2].Title == "Bob-Third");

            // COMMENT CHECKS
            Assert.True(result[0].Posts[1].Comments.Count == 4);
            Assert.True(result[1].Posts[2].Comments.Count == 1);

            Assert.True(result[0].Posts[1].Comments[0].Content == "Radu-Second-C1");
            Assert.True(result[0].Posts[1].Comments[1].Content == "Radu-Second-C2");
            Assert.True(result[0].Posts[1].Comments[2].Content == "Radu-Second-C3");
            Assert.True(result[0].Posts[1].Comments[3].Content == "Radu-Second-C4");

            Assert.True(result[1].Posts[2].Comments[0].Content == "Bob-Third-C1");
        }
    }
}
