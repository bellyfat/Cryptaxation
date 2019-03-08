﻿using System.Collections.Generic;
using Cryptaxation.Pdf.Models;

namespace Cryptaxation.Pdf.Contract
{
    public interface IK4FormLogic
    {
        List<K4FillModel> GetK4FillModelList();
         K4TabIndexModel GetTabIndexesByYear(int year);
    }
}
