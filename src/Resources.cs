using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;

public class Resources
{
	private static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
	private static HashSet<String> ValidTextureExtensions = new HashSet<string>
	{
		".png",
		".bmp",
	};

	public static Texture GetTexture(string name)
	{
		return textures[name];
	}

	private static void AddDirectory(VirtualFolder root, DirectoryInfo dir)
	{
		foreach(var file in dir.GetFiles())
		{
			root.AddEntry(new VirtualRealFile(file.Name, file.FullName));
		}
		foreach(var realFolder in dir.GetDirectories())
		{
			VirtualFolder folder = new VirtualFolder(realFolder.Name);
			folder.Parent = root;
			root.AddEntry(folder);
			AddDirectory(folder, realFolder);
		}
	}

	public static void LoadPackageFromDir(VirtualFileSystem vfs, string path)
	{
		DirectoryInfo root = new DirectoryInfo(path);

		AddDirectory(vfs.GetFolder("/"), root);
	}

	public static void LoadPackage(VirtualFileSystem vfs, string path)
	{
		ZipArchive archive = ZipFile.OpenRead(path);

		foreach(var entry in archive.Entries)
		{
			var filepath = "/" + entry.FullName;
			if(entry.Name == "")
			{
				vfs.GetFolder(filepath);
			} else
			{
				vfs.AddFile(filepath, new VirtualZipFile(entry.Name, entry));
			}
		}
	}

	private static void LoadTextures(VirtualFileSystem vfs)
	{
		var renderer = Renderer.Instance;
		var folder = vfs.GetFolder("/textures");

		foreach (var entry in folder)
		{
			if (entry.IsFile)
			{
				VirtualFile file = (VirtualFile) entry;
				string extension = Path.GetExtension(file.Name);
				if (ValidTextureExtensions.Contains(extension))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name);
					var stream = file.Open(out long streamLength);
					textures.Add(name, renderer.LoadTextureFromStream(stream, file.Name, streamLength));
					stream.Close();
				}
			}
		}
	}

	public static Task Load()
	{
		Task t = new Task(() =>
		{
			var vfs = new VirtualFileSystem();
			LoadPackageFromDir(vfs, "data/");
			//LoadPackage(vfs, "extra.zip");
			LoadTextures(vfs);
		});
		t.Start();
		return t;
	}
}

// Junkyard pls dont remove

/*var dirs = Directory.GetDirectories(path);

var renderer = Renderer.Instance;

foreach (var folderPath in dirs)
{
	var folderName = Path.GetFileName(folderPath);
	switch(folderName)
	{
	case "textures":
	{
		var textureFiles = Directory.GetFiles(folderPath);
		foreach (var texturePath in textureFiles)
		{
			var textureName = Path.GetFileNameWithoutExtension(texturePath);
			var extension = Path.GetExtension(texturePath);

			Console.WriteLine("extension: " + extension);
			if (ValidTextureExtensions.Contains(extension))
			{
				Console.WriteLine("name: " + textureName + ", path: " + texturePath);
				textures.Add(textureName, renderer.LoadTexture(texturePath));
			}
		}
	} break;
	}
}*/
