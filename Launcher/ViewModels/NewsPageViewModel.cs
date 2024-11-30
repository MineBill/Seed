using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels;

public partial class NewsPageViewModel : PageViewModel
{
    private readonly IFilesService _files;

    [ObservableProperty]
    private ObservableCollection<NewsItem> _items = [];

    public NewsPageViewModel(Func<List<NewsItem>> itemsFactory, IFilesService files)
    {
        PageName = PageNames.News;
        _files = files;
        Items = new ObservableCollection<NewsItem>(itemsFactory.Invoke());
    }

    [RelayCommand]
    private void OpenInBrowser(string url)
    {
        if (url == string.Empty) return;
        _files.OpenUri(new Uri(url));
    }
}