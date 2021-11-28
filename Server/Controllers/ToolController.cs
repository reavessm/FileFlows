namespace FileFlows.Server.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using FileFlows.Server.Helpers;
    using FileFlows.Shared.Models;

    [Route("/api/tool")]
    public class ToolController : Controller
    {
        [HttpGet]
        public IEnumerable<Tool> GetAll()
        {
            if(Globals.Demo)
                return new [] { new Tool() { Name = "FFMPEG", Path = "/var/lib/ffmpeg", Uid = Guid.NewGuid() } };
            return DbHelper.Select<Tool>();
        }

        [HttpGet("{uid}")]
        public Tool Get(Guid uid)
        {
            if (Globals.Demo)
                return new Tool() { Name = "FFMPEG", Path = "/var/lib/ffmpeg", Uid = uid };

            return DbHelper.Single<Tool>(uid);
        }

        [HttpGet("{uid}")]
        public Tool GetByName(string name)
        {
            if (Globals.Demo)
                return new Tool() { Name = name, Path = "/var/lib/ffmpeg", Uid = Guid.NewGuid() };

            return DbHelper.SingleByName<Tool>(name);
        }

        [HttpPost]
        public Tool Save([FromBody] Tool tool)
        {
            if (Globals.Demo)
                return tool;

            var duplicate = DbHelper.Single<Tool>("lower(name) = lower(@1) and uid <> @2", tool.Name, tool.Uid.ToString());
            if (duplicate != null && duplicate.Uid != Guid.Empty)
                throw new Exception("ErrorMessages.NameInUse");

            return DbHelper.Update(tool);
        }

        [HttpDelete]
        public void Delete([FromBody] ReferenceModel model)
        {
            if (Globals.Demo)
                return;

            if (model == null || model.Uids?.Any() != true)
                return; // nothing to delete
            DbHelper.Delete<Tool>(model.Uids);
        }
    }
}