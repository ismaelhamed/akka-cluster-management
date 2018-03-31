using System;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Akka.Actor;

#pragma warning disable 1573

namespace Akka.Cluster.Http.Management.Controllers
{
    [RoutePrefix("")]
    public class ClusterHttpManagementController : ApiController
    {
        /// <summary>
        /// Returns the status of the Cluster
        /// </summary>
        [Route("members"), HttpGet]
        public async Task<IHttpActionResult> GetMembers()
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new GetMembers(), TimeSpan.FromSeconds(5));
                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(success.Result))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns the status of {address} in the Cluster.
        /// </summary>
        /// <param name="address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [Route("members"), HttpGet]
        public async Task<IHttpActionResult> GetMembers([FromUri(Name = "address")] string address)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new GetMember(Address.Parse(address)), TimeSpan.FromSeconds(5));
                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(success.Result))
                    .With<Complete.Failure>(failure => Content(HttpStatusCode.NotFound, new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes join operation in cluster for the provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [Route("members"), HttpPost]
        public async Task<IHttpActionResult> PostMembers(FormDataCollection formData)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new JoinMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes leave operation in cluster for provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [Route("members"), HttpDelete]
        public async Task<IHttpActionResult> DeleteMember(FormDataCollection formData)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new LeaveMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .With<Complete.Failure>(failure => Content(HttpStatusCode.NotFound, new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes down/leave operation in cluster for provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        /// <param name="formData.operation">Expected values are 'Down' or 'Leave'</param>
        [Route("members"), HttpPut]
        public async Task<IHttpActionResult> PutMember(FormDataCollection formData)
        {
            try
            {
                Complete response;

                switch (formData["operation"])
                {
                    case "down":
                        response = await SystemActors.RoutesHandler.Ask<Complete>(new DownMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                        break;
                    case "leave":
                        response = await SystemActors.RoutesHandler.Ask<Complete>(new LeaveMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                        break;
                    default:
                        return BadRequest("Operation not supported.");
                }

                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .With<Complete.Failure>(failure => Content(HttpStatusCode.NotFound, new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns shard info for the shard region with the provided {name}
        /// </summary>
        [Route("shards/{name}"), HttpGet]
        public async Task<IHttpActionResult> GetShardInfo([FromUri(Name = "name")] string name)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new GetShardInfo(name), TimeSpan.FromSeconds(5));
                return response.Match<IHttpActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .With<Complete.Failure>(failure => Content(HttpStatusCode.NotFound, new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}