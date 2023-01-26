using System;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Akka.Cluster.Management.Controllers
{
    [Route("")]
    public class ClusterHttpManagementController : ControllerBase
    {
        /// <summary>
        /// Returns the status of {address} in the Cluster.
        /// </summary>
        /// <param name="address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [HttpGet("members")]
        public async Task<IActionResult> GetMembers([FromQuery] string address)
        {
            try
            {
                var response = string.IsNullOrEmpty(address)
                    ? await SystemActors.RoutesHandler.Ask<Complete>(new GetMembers(), TimeSpan.FromSeconds(5))
                    : await SystemActors.RoutesHandler.Ask<Complete>(new GetMember(Address.Parse(address)), TimeSpan.FromSeconds(5));

                return response.Match<IActionResult>()
                    .With<Complete.Success>(success => Ok(success.Result))
                    .With<Complete.Failure>(failure => NotFound(new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Executes join operation in cluster for the provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [HttpPost("members")]
        public async Task<IActionResult> PostMembers(IFormCollection formData)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new JoinMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                return response.Match<IActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Executes leave operation in cluster for provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        [HttpDelete("members")]
        public async Task<IActionResult> DeleteMember(IFormCollection formData)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new LeaveMember(Address.Parse(formData["address"])), TimeSpan.FromSeconds(5));
                return response.Match<IActionResult>()
                    .With<Complete.Success>(success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())))
                    .With<Complete.Failure>(failure => NotFound(new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Executes down/leave operation in cluster for provided {address}.
        /// </summary>
        /// <param name="formData.address">The expected format of address follows the Cluster URI convention. Example: akka://Main@myhostname.com:3311</param>
        /// <param name="formData.operation">Expected values are 'Down' or 'Leave'</param>
        [HttpPut("members")]
        public async Task<IActionResult> PutMember(IFormCollection formData)
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

                return response switch
                {
                    Complete.Success success => Ok(new ClusterHttpManagementMessage(success.Result.ToString())),
                    Complete.Failure failure => NotFound(new ClusterHttpManagementMessage(failure.Reason)),
                    _ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."),
                };
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns shard info for the shard region with the provided {name}
        /// </summary>
        [HttpGet("shards/{name}")]
        public async Task<IActionResult> GetShardInfo(string name)
        {
            try
            {
                var response = await SystemActors.RoutesHandler.Ask<Complete>(new GetShardInfo(name), TimeSpan.FromSeconds(5));
                return response.Match<IActionResult>()
                    .With<Complete.Success>(success => Ok(success.Result))
                    .With<Complete.Failure>(failure => NotFound(new ClusterHttpManagementMessage(failure.Reason)))
                    .ResultOrDefault(_ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}