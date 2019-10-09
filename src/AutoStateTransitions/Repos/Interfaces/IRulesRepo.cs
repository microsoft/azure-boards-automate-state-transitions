using AutoStateTransitions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoStateTransitions.Repos.Interfaces
{
    public interface IRulesRepo
    {
        Task<RulesModel> ListRules(string wit);
    }
}