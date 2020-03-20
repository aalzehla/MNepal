using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;
using MNepalProject.Models;

namespace MNepalProject.Services.Validations.DBModels
{
    public class MNComAndFocusOneLogValidator:AbstractValidator<MNComAndFocusOneLog>{
        public MNComAndFocusOneLogValidator()
        {
            RuleFor(mnComAndFocusOneLog => mnComAndFocusOneLog.InOutFlag).NotEmpty().WithMessage("Please specify IN Or OUT relative to Com");
        }
    }
}