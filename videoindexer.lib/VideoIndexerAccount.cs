using System;

namespace videoindexer.lib
{
	public class VideoIndexerAccount
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string AccountType { get; set; }
		public string Url { get; set; }
		public string AccessToken { get; set; }
	}
}
