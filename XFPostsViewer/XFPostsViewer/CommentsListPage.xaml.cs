﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFPostsViewer.Data;
using XFPostsViewer.Service;

namespace XFPostsViewer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentsListPage : ContentPage
    {
        public ObservableCollection<Comment> CommentsList { get; }

        DataRetriever _dataRetriever;
        Post _postContext;
        Comment _comment;

        public CommentsListPage()
        {
            InitializeComponent();

            StartActivityIndicator();
        }

        public CommentsListPage(Post post) : this()
        {
            BindingContext = post;
            _postContext = BindingContext as Post;

            Title = "Comments";

            CommentsList = new ObservableCollection<Comment>();
            _dataRetriever = new DataRetriever();
            CommentsListView.ItemsSource = CommentsList;

            LoadComments();

            CommentsListView.ItemSelected += CommentsListView_ItemSelected;
            CommentsListView.RefreshCommand = new Command(() =>
            {
                LoadComments();
                CommentsListView.IsRefreshing = false;
            });
        }

        private async void LoadComments()
        {
            try
            {
                List<Comment> comments = await _dataRetriever.GetCommentsByPostAsync(_postContext.PostId);
                CommentsList.Clear();
                foreach (Comment comment in comments)
                {
                    CommentsList.Add(comment);

                    if (CommentsList.Count > 0)
                    {
                        StopActivityIndicator();
                    }
                }
            }
            catch (WebException)
            {
                StopActivityIndicator();
                await DisplayAlert("Not Connected", "You are not connected to the Internet. Please Connect and Pull down the page to Refresh", "OK");
            }
        }

        private void StartActivityIndicator()
        {
            CommentsLoaderIndicator.IsRunning = true;
            CommentsLoaderIndicator.IsVisible = true;
        }

        private void StopActivityIndicator()
        {
            CommentsLoaderIndicator.IsVisible = false;
            CommentsLoaderIndicator.IsRunning = false;
        }

        private async void SendEmailDialog()
        {
            var answer = await DisplayAlert("Send Email", "Do you want to send an email to this commenter?", "Send", "No");
            if (answer)
            {
                SendEmail();
            }
        }

        private void SendEmail()
        {
            string mailto = "mailto:" + _comment.Email;
            Device.OpenUri(new Uri(mailto));
        }

        private void CommentsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                _comment = e.SelectedItem as Comment;
                SendEmailDialog();
            }

            CommentsListView.SelectedItem = null;
        }

        private void UserIcon_Activated(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AuthorProfilePage(_postContext.UserId));
        }
    }
}