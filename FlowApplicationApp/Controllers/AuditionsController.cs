using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Infrastructure.Extensions;
using FlowApplicationApp.ViewModels.Auditions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Controllers
{
    [Route("[controller]")]
    [AutoValidateAntiforgeryToken]
    public sealed class AuditionsController : Controller
    {
        private readonly ILogger<AuditionsController> _logger;
        private readonly IValidator<CreateAuditionerInputModel> _validator;

        public AuditionsController(
            ILogger<AuditionsController> logger,
            IValidator<CreateAuditionerInputModel> validator
        )
        {
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuditioner([Bind]CreateAuditionerInputModel inputModel)
        {
            if (!ModelState.IsValid) return View("Index", inputModel);
            var validationResult = await _validator.ValidateAsync(inputModel);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View("Index", inputModel);
            }
            //save every image to the img folder and save the path to the database
            var entity = inputModel.MapToDomainModel();
            return View();
        }
    }
}