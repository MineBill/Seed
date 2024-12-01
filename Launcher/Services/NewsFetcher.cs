using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace Launcher.Services;

public record NewsItem(string Title, string BackgroundUrl, string NewsPageUrl, DateTime PublishDate)
{
    public string ShortDate => PublishDate.ToShortDateString();
}

public partial class NewsFetcher
{
    public List<NewsItem> Items { get; } = [];

    public NewsFetcher()
    {
        Rss20FeedFormatter rssFormatter;

        using (var xmlReader = XmlReader.Create
                   ("https://flaxengine.com/feed/"))
        {
            rssFormatter = new Rss20FeedFormatter();
            rssFormatter.ReadFrom(xmlReader);
        }

        var matcher = ImageSrcMatcher();

        foreach (var item in rssFormatter.Feed.Items)
        {
            var match = matcher.Match(item.Summary.Text);
            if (match.Groups.Count < 2) continue;
            var url = match.Groups[1].Value;


            Items.Add(new NewsItem(item.Title.Text, url, item.Links[0].Uri.AbsoluteUri, item.PublishDate.Date));
        }
    }

    [GeneratedRegex("""src=\"(.*?)(?=\")""")]
    private static partial Regex ImageSrcMatcher();
}