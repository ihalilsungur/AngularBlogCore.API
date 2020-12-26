﻿using System;
using System.Collections.Generic;

namespace AngularBlogCore.API.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string ContentMain { get; set; }
        public DateTime PublishDate { get; set; }

        public Article Article { get; set; }
    }
}
