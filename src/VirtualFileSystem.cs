using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Collections;

public class VirtualRealFile : VirtualFile
{
	public string FullPath { get; }

	public VirtualRealFile(string name, string fullPath) : base()
	{
		this.Name = name;
		this.FullPath = fullPath;
	}

	public override Stream Open(out long length)
	{
		var fs = new FileStream(FullPath, FileMode.Open);
		Trace.Assert(fs.CanSeek && fs.Length != 0);
		length = fs.Length;
		return fs;
	}
}

public class VirtualZipFile : VirtualFile
{
	public ZipArchiveEntry entry { get; }

	public VirtualZipFile(string name, ZipArchiveEntry entry) : base()
	{
		Name = name;
		this.entry = entry;
	}

	public override Stream Open(out long length)
	{
		length = entry.Length;
		return entry.Open();
	}
}

public abstract class VirtualFile : VirtualEntry
{
	public VirtualFile() : base(true) { }

	public abstract Stream Open(out long length);
}

public class VirtualFolder : VirtualEntry, IEnumerable<VirtualEntry>
{
	private List<VirtualEntry> Entries;

	public VirtualFolder(string name) : base(false)
	{
		Name = name;
		Entries = new List<VirtualEntry>();
	}

	public void AddEntry(VirtualEntry entry)
	{
		entry.Parent = this;
		Entries.Add(entry);
	}

	public VirtualEntry GetEntry(string name)
	{
		foreach (var e in Entries)
		{
			if (e.Name == name) return e;
		}
		return null;
	}

	public IEnumerator<VirtualEntry> GetEnumerator()
	{
		return ((IEnumerable<VirtualEntry>)Entries).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<VirtualEntry>)Entries).GetEnumerator();
	}

	public VirtualFolder GetFolder(string path)
	{
		VirtualFolder result = this;
		Trace.Assert(path.StartsWith("/"));

		var parts = path.Split('/');
		for (int i = 1; i < parts.Length; i++)
		{
			string folderName = parts[i];

			if (folderName == "")
			{
				if (i == parts.Length - 1)
				{
					return result;
				}
				else
				{
					Trace.Assert(false, "Empty path name in the middle of path: " + path);
				}
			}

			VirtualEntry entry = result.GetEntry(folderName);
			if (entry == null)
			{
				var new_folder = new VirtualFolder(folderName);
				new_folder.Parent = result;
				result.Entries.Add(new_folder);
				result = new_folder;
			}
			else if (entry.IsDirectory)
			{
				result = (VirtualFolder)entry;
			}
			else
			{
				Trace.Assert(false, "File name in the middle of the path: " + path);
			}
		}


		return result;
	}
}

public class VirtualEntry
{
	public String Name { get; set; }
	public VirtualFolder Parent { get; set; }
	public bool IsFile { get; }
	public bool IsDirectory { get { return !IsFile; } }

	public VirtualEntry(bool IsFile)
	{
		this.IsFile = IsFile;
		this.Parent = Parent;
	}
}

public class VirtualFileSystem
{
	private VirtualFolder Root { get; }

	public VirtualFileSystem()
	{
		Root = new VirtualFolder("root");
	}

	public void AddFile(string path, VirtualFile file)
	{
		int last = path.LastIndexOf('/');
		string dir = path.Substring(0, last+1);
		var folder = GetFolder(dir);
		folder.AddEntry(file);
	}

	public VirtualFile GetFile(string path)
	{
		int last = path.LastIndexOf('/');
		string dir = path.Substring(0, last+1);
		var folder = GetFolder(dir);
		string name = path.Substring(last+1);
		var entry = folder.GetEntry(name);

		if (entry != null && entry.IsFile)
		{
			return (VirtualFile)entry;
		}
		else
		{
			return null;
		}
	}

	public VirtualFolder GetFolder(string path)
	{
		return Root.GetFolder(path);
	}
}