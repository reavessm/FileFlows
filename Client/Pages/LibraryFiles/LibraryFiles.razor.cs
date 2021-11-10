namespace FileFlow.Client.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Radzen.Blazor;
    using FileFlow.Client.Components;
    using FileFlow.Client.Helpers;
    using FileFlow.Shared;
    using FileFlow.Shared.Models;
    using Radzen;
    using System.Linq;
    using FileFlow.Client.Components.Dialogs;

    public partial class LibraryFiles : ListPage<LibraryFile>
    {
        public override string ApIUrl => "/api/library-file";

        private FileFlow.Shared.Models.FileStatus SelectedStatus = FileFlow.Shared.Models.FileStatus.Unprocessed;

        private readonly List<LibraryStatus> Statuses = new List<LibraryStatus>();

        private void SetSelected(LibraryStatus status)
        {
            SelectedStatus = status.Status;
            this.StateHasChanged();
            _ = this.Refresh();
        }

        public override string FetchUrl => ApIUrl + "?status=" + SelectedStatus;

        public override async Task PostLoad()
        {
            await RefreshStatus();
        }
        protected override async Task PostDelete()
        {
            await RefreshStatus();
        }

        private async Task RefreshStatus()
        {
            var result = await HttpHelper.Get<List<LibraryStatus>>(ApIUrl + "/status");
            if (result.Success)
            {
                var order = new List<FileStatus> { FileStatus.Unprocessed, FileStatus.Processing, FileStatus.Processed, FileStatus.FlowNotFound, FileStatus.ProcessingFailed };
                foreach (var s in order)
                {
                    if (result.Data.Any(x => x.Status == s) == false && s != FileStatus.FlowNotFound)
                        result.Data.Add(new LibraryStatus { Status = s });

                }
                foreach (var s in result.Data)
                    s.Name = Translater.Instant("Enums.FileStatus." + s.Status.ToString());
                Statuses.Clear();
                Statuses.AddRange(result.Data.OrderBy(x => { int index = order.IndexOf(x.Status); return index >= 0 ? index : 100; }));
            }
        }

        public override async Task Edit(LibraryFile item)
        {
            await Helpers.LibraryFileEditor.Open(Blocker, NotificationService, Editor, item);
        }
    }
}