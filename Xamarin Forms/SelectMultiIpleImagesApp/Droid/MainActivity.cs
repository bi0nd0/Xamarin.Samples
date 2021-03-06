﻿using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Android.Database;
using System.Collections.Generic;
using Xamarin.Forms;
using DLToolkit.Forms.Controls;
using FFImageLoading.Forms.Droid;
using Android.Graphics;
using Android.Media;
using System.IO;
using SelectMultiIpleImagesApp.Droid.Helpers;

namespace SelectMultiIpleImagesApp.Droid
{
	[Activity(Label = "SelectMultiIpleImagesApp.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		public static int OPENGALLERYCODE = 100;
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			FlowListView.Init();
			CachedImageRenderer.Init();

			LoadApplication(new App());
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == OPENGALLERYCODE && resultCode == Result.Ok)
			{
				List<string> images = new List<string>();

				if (data != null)
				{
					ClipData clipData = data.ClipData;
					if (clipData != null)
					{
						for (int i = 0; i < clipData.ItemCount; i++)
						{
							ClipData.Item item = clipData.GetItemAt(i);
							Android.Net.Uri uri = item.Uri;
							var path = GetRealPathFromURI(uri);

							if (path != null)
							{
								//Rotate Image
								var imageRotated = ImageHelpers.RotateImage(path);
								var newPath = ImageHelpers.SaveFile("TmpPictures", imageRotated, System.DateTime.Now.ToString("yyyyMMddHHmmssfff"));
								images.Add(newPath);
							}
						}
					}
					else
					{
						Android.Net.Uri uri = data.Data;
						var path = GetRealPathFromURI(uri);

						if (path != null)
						{
							//Rotate Image
							var imageRotated = ImageHelpers.RotateImage(path);
							var newPath = ImageHelpers.SaveFile("TmpPictures", imageRotated, System.DateTime.Now.ToString("yyyyMMddHHmmssfff"));
							images.Add(newPath);
						}
					}

					MessagingCenter.Send<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelected", images);
				}
			}
		}



		public String GetRealPathFromURI(Android.Net.Uri contentURI)
		{
			try
			{
				ICursor imageCursor = null;
				string fullPathToImage = "";

				imageCursor = ContentResolver.Query(contentURI, null, null, null, null);
				imageCursor.MoveToFirst();
				int idx = imageCursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

				if (idx != -1)
				{
					fullPathToImage = imageCursor.GetString(idx);
				}
				else
				{
					ICursor cursor = null;
					var docID = DocumentsContract.GetDocumentId(contentURI);
					var id = docID.Split(':')[1];
					var whereSelect = MediaStore.Images.ImageColumns.Id + "=?";
					var projections = new string[] { MediaStore.Images.ImageColumns.Data };

					cursor = ContentResolver.Query(MediaStore.Images.Media.InternalContentUri, projections, whereSelect, new string[] { id }, null);
					if (cursor.Count == 0)
					{
						cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, projections, whereSelect, new string[] { id }, null);
					}
					var colData = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
					cursor.MoveToFirst();
					fullPathToImage = cursor.GetString(colData);
				}
				return fullPathToImage;
			}
			catch (Exception ex)
			{
				Toast.MakeText(Xamarin.Forms.Forms.Context, "Unable to get path", ToastLength.Long).Show();

			}

			return null; 

		}
	}
}
