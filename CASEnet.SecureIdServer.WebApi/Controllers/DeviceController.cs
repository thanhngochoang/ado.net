using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CASEnet.SecureIdServer.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CASEnet.SecureIdServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger _logger;

        public DeviceController(IDeviceService deviceService, ILoggerFactory loggerFactory)
        {
            _deviceService = deviceService;
            _logger = loggerFactory.CreateLogger<DeviceController>();
        }

        /// <summary>
        /// Regiser Device
        /// </summary>
        /// <param name="phone">phone Number</param>
        /// <returns>Return the installation ID of the newly confirmation</returns>
        /// <response code="200">Return the installation ID of the newly confirmation with send a mesaenger to phone number</response>
        /// <response code="400">Phone number incorrect</response>
        /// <response code="500">Server error</response>
        [HttpPost()]
        public ActionResult<Guid> RegisterDevice([FromBody] string phone)
        {
            try
            {
                var code = Extentions.RandomDigit();
                return _deviceService.RegisterDevice(phone, code);
            }
            catch (ArgumentException)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error:::::", ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Confirm deivce registration
        /// </summary>
        /// <param name="installationId"></param>
        /// <param name="confirmCode"></param>
        /// <returns>StatusCode</returns>
        /// <response code="200">Success</response>
        /// <response code="400">Param string invalid</response>
        /// <response code="403">Confirm code  invalid</response>
        /// <response code="404">Installation not exists</response>
        /// <response code="500">Installation had confirmed</response>
        [HttpPut("{installationId}")]
        public ActionResult ConfirmDeviceRegistration(Guid installationId, string confirmCode)
        {
            try
            {
                var status = _deviceService.ConfirmDeviceRegistration(installationId, confirmCode);
                return StatusCode(status);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error:::::", ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Request get device code
        /// </summary>
        /// <param name="installationId"></param>
        /// <returns>Device code</returns>
        /// <response code="200">Device code</response>
        /// <response code="404">Not found code for device</response>
        /// <response code="500">Server error</response>
        [HttpGet("{installationId}")]
        public IActionResult RequestDeviceCode(Guid installationId)
        {
            try
            {
                var code = _deviceService.RequestDeviceCode(installationId);
                return Ok(code);
            }
            catch (ArgumentNullException)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error:::::", ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update new code
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        /// <response code="200">Generate new code</response>
        /// <response code="500">Server error</response>
        [HttpPut("")]
        public IActionResult UpdateDeviceCode([FromBody] string phone)
        {
            try
            {
                _deviceService.UpdateDeviceCode(phone, Extentions.RandomDigit());
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error:::::", ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

    }
}