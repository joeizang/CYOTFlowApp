using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.ViewModels.Auditions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Controllers
{
    [Route("[controller]")]
    public class AuditionsController : Controller
    {
        private readonly ILogger<AuditionsController> _logger;

        public AuditionsController(ILogger<AuditionsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public IActionResult CreateAuditioner([Bind]CreateAuditionerInputModel inputModel)
        {
            var testing = inputModel;
            return View();
        }
    }
}