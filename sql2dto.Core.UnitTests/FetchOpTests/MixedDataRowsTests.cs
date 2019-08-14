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
        }

        [Fact]
        public void Mixed_parent_rows_with_children()
        {
            var fakeReader = new FakeDataReader("BlogId", "BlogAuthor", "PostId", "PostTitle");
            fakeReader.AddRow(1, "Radu", 100, "Radu-First");
            fakeReader.AddRow(1, "Radu", 200, "Radu-Second");
            fakeReader.AddRow(2, "Bob", 300, "Bob-First");
            fakeReader.AddRow(1, "Radu", 300, "Radu-Third");

            var h = new ReadHelper(fakeReader);

            var fetch =
                h.Fetch<Blog>()
                    .Include<Post>((blog, post) => { blog.Posts.Add(post); });

            var result = fetch.All();

            //Assert.True(result.Count == 2);
            //Assert.True(result[0].Author == "Radu");
            //Assert.True(result[1].Author == "Bob");

            //Assert.True(result[0].Posts.Count == 3);
            //Assert.True(result[1].Posts.Count == 1);

            //Assert.True(result[1].Posts[0].Title == "Bob-First");

            //Assert.True(result[0].Posts[0].Title == "Radu-First");
            //Assert.True(result[0].Posts[1].Title == "Radu-Second");
            //Assert.True(result[0].Posts[2].Title == "Radu-Third");

            //select
            //*
            //from
            //(
            //	select 1 'a', 10000 'b'
            //	union
            //	select 2, 50
            //	union
            //	select 1, 20000
            //	union
            //	select 2, 20
            //	union
            //	select 1, 40000
            //	union
            //	select 2, 10
            //	union
            //	select 1, 30000
            //) as X
            //order by X.b desc;
        }
    }
}
