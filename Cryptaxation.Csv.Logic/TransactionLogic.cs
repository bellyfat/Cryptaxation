﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cryptaxation.Csv.Contract;
using Cryptaxation.Csv.Fields;
using Cryptaxation.Entities;
using Cryptaxation.Entities.Types;
using Cryptaxation.Entities.Types.Enums;
using Microsoft.VisualBasic.FileIO;

namespace Cryptaxation.Csv.Logic
{
    public class TransactionLogic<T> : ITransactionLogic<T> where T : Transaction, new()
    {
        private readonly List<Rate> _rates;

        public TransactionLogic(List<Rate> rates)
        {
            _rates = rates;
        }

        public List<T> CreateTransactionList(string path)
        {
            List<T> transactionList = new List<T>();

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                for (int rowIndex = 0; !parser.EndOfData; rowIndex++)
                {
                    string[] row = parser.ReadFields();
                    if (rowIndex == 0)
                    {
                        continue;
                    }
                    if (row != null)
                    {
                        T transaction = CreateTransaction(row);
                        if (transaction.Type == TransactionType.Deposit && transaction.Amount.Type == CurrencyType.FiatCurrency)
                        {
                            transaction.Action= TradeAction.Buy;
                        }
                        if (transaction.Type == TransactionType.Withdrawal && transaction.Amount.Type == CurrencyType.FiatCurrency)
                        {
                            transaction.Action = TradeAction.Sell;
                        }
                        if ((transaction.Type == TransactionType.Deposit || transaction.Type == TransactionType.Withdrawal) && transaction.Amount.Type == CurrencyType.FiatCurrency)
                        {
                            List<Rate> originRates = _rates.Where(r => r.OriginCurrency == CurrencyCode.SEK && r.DestinationCurrency == transaction.Amount.CurrencyCode && r.Date <= transaction.DateTime).ToList();
                            decimal rate = originRates.FirstOrDefault().Value;
                            transaction.Type = TransactionType.Market;
                            transaction.Value = new Currency
                            {
                                Value = transaction.Amount.Value / rate,
                                CurrencyCode = CurrencyCode.SEK
                            };
                            transaction.Rate = new Currency
                            {
                                Value = rate,
                                CurrencyCode = CurrencyCode.SEK
                            };
                            transaction.Fee = new Currency
                            {
                                Value = 0m,
                                CurrencyCode = CurrencyCode.SEK
                            };
                        }
                        transactionList.Add(transaction);
                    }
                    else
                    {
                        throw new Exception("No rows found.");
                    }
                }
            }
            return transactionList;
        }

        public T CreateTransaction(string[] row)
        {
            T transaction = new T();
            for (int i = 0; i < row.Length; i++)
            {
                switch ((TransactionFields)i)
                {
                    case TransactionFields.Type:
                        if (!string.IsNullOrWhiteSpace(row[i]))
                        {
                            transaction.Type = (TransactionType)Enum.Parse(typeof(TransactionType), row[i], true);
                        }
                        break;
                    case TransactionFields.Datetime:
                        transaction.DateTime = DateTime.ParseExact(row[i], "MMM. dd, yyyy, hh:mm tt", CultureInfo.InvariantCulture);
                        break;
                    case TransactionFields.Account:
                        transaction.Account = row[i];
                        break;
                    case TransactionFields.Amount:
                        transaction.Amount = ConvertFieldToCurrency(row[i]);
                        break;
                    case TransactionFields.Value:
                        transaction.Value = ConvertFieldToCurrency(row[i]);
                        break;
                    case TransactionFields.Rate:
                        transaction.Rate = ConvertFieldToCurrency(row[i]);
                        break;
                    case TransactionFields.Fee:
                        transaction.Fee = ConvertFieldToCurrency(row[i]);
                        break;
                    case TransactionFields.Action:
                        if (!string.IsNullOrWhiteSpace(row[i]))
                        {
                            transaction.Action = (TradeAction)Enum.Parse(typeof(TradeAction), row[i], true);
                        }
                        break;
                    default:
                        throw new Exception("Invalid field");
                }
            }
            return transaction;
        }

        private Currency ConvertFieldToCurrency(string field, CultureInfo cultureInfo = null, NumberStyles numberStyle = NumberStyles.Any)
        {
            if (!string.IsNullOrWhiteSpace(field))
            {
                if (cultureInfo == null)
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }
                string[] valueCurrency = field.Split(' ');
                return new Currency
                {
                    Value = decimal.Parse(valueCurrency[0], numberStyle, cultureInfo),
                    CurrencyCode = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), valueCurrency[1], true)
                };
            }
            return new Currency();
        }
    }
}
