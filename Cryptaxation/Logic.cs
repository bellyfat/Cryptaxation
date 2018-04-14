﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptaxation.Helpers;
using Cryptaxation.Models;

namespace Cryptaxation
{
    public class Logic
    {
        private string _fullName;
        private string _personNumber;
        private string _bitstampTransactionsPath;
        private string _ratesPath;
        private string _k4Path;
        private string _outputPath;
        private string _processName;
        private CsvHelper _csvHelper;
        private PdfHelper _pdfHelper;
        private TransactionHelper _transactionHelper;

        public Logic(string fullName, string personNumber, string bitstampTransactionsPath, string ratesPath, string k4Path, string outputPath, string processName)
        {
            _fullName = fullName;
            _personNumber = personNumber;
            _bitstampTransactionsPath = bitstampTransactionsPath;
            _ratesPath = ratesPath;
            _k4Path = k4Path;
            _outputPath = outputPath;
            _processName = processName;

            ValidateInput();

            _csvHelper = new CsvHelper();
            _pdfHelper = new PdfHelper(_processName);

            
        }

        public void ValidateInput()
        {
            ValidateFullName();
            ValidatePersonNumber();
        }

        public void ValidateFullName()
        {
            if (string.IsNullOrWhiteSpace(_fullName))
            {
                throw new Exception("Invalid full name.");
            }
        }

        public void ValidatePersonNumber()
        {
            // TODO
            if (string.IsNullOrWhiteSpace(_personNumber))
            {
                throw new Exception("Invalid person number.");
            }
        }

        public void Execute()
        {
            List<BitstampTransaction> bitstampTransactionList = _csvHelper.CreateBitstampTransactionList(_bitstampTransactionsPath);
            List<Rate> rateList = _csvHelper.CreateRateList(_ratesPath);
        }
    }
}
