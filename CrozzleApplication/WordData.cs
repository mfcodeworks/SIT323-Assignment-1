using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CrozzleApplication
{
    class WordData
    {
        #region constants
        public const String OrientationRow = "ROW";
        public const String OrientationColumn = "COLUMN";
        public const int NumberOfFields = 4;
        #endregion

        #region properties - errors
        public static List<String> Errors { get; set; }
        #endregion

        #region properties
        private String[] OriginalWordData { get; set; }
        public Boolean Valid { get; set; } = false;
        public Orientation Orientation { get; set; }
        public Coordinate Location { get; set; }
        public String Letters { get; set; }
        #endregion

        #region properties - testing
        public Boolean IsHorizontal
        {
            get { return (Orientation.IsHorizontal); }
        }

        public Boolean IsVertical
        {
            get { return (Orientation.IsVertical); }
        }
        #endregion

        #region constructors
        public WordData(String[] originalWordDataData)
        {
            OriginalWordData = originalWordDataData;
        }

        public WordData(String direction, int row, int column, String sequence)
        {
            OriginalWordData = new String[] { direction, row.ToString(), column.ToString(), sequence};
            Orientation anOrientation;
            if (!Orientation.TryParse(direction, out anOrientation))
                Errors.AddRange(CrozzleApplication.Orientation.Errors);
            Orientation = anOrientation;
            Location = new Coordinate(row, column);
            Letters = sequence;
        }
        #endregion

        #region parsing
        public static Boolean TryParse(String originalWordDataData, Crozzle aCrozzle, out WordData aWordData)
        {
            String[] originalWordData = originalWordDataData.Split(new Char[] { '=', ',' });

            Errors = new List<String>();
            aWordData = new WordData(originalWordData);

            // Check that the original word data has the correct number of fields.
            if (originalWordData.Length != NumberOfFields)
                Errors.Add(String.Format(WordDataErrors.FieldCountError, originalWordData.Length, originalWordDataData, NumberOfFields));
            
            // Check that each field is not empty.
            for (int field = 0; field < originalWordData.Length; field++)
                if (originalWordData[field].Length == 0)
                    Errors.Add(String.Format(WordDataErrors.BlankFieldError, originalWordDataData, field));

            if (originalWordData.Length > 0)
            {
                // Check that the 1st field is an orientation value.
                Orientation anOrientation;
                if (!Orientation.TryParse(originalWordData[0], out anOrientation))
                    Errors.AddRange(Orientation.Errors);
                aWordData.Orientation = anOrientation;

                if (anOrientation.Valid)
                {
                    // Check that the 2nd and 4th fields are a Coordinate.
                    if (originalWordData.Length >= NumberOfFields)
                    {
                        String rowValue = "";
                        String columnValue = "";
                        if (anOrientation.IsHorizontal)
                        {
                            rowValue = originalWordData[1];
                            columnValue = originalWordData[3];
                        }
                        else if (anOrientation.IsVertical)
                        {
                            rowValue = originalWordData[3];
                            columnValue = originalWordData[1];
                        }

                        if (rowValue.Length > 0 && columnValue.Length > 0)
                        {
                            Coordinate aCoordinate;
                            if (!Coordinate.TryParse(rowValue, columnValue, aCrozzle, out aCoordinate))
                                Errors.AddRange(Coordinate.Errors);
                            aWordData.Location = aCoordinate;
                        }
                    }

                    // Check that the 3rd field is alphabetic, and in the wordlist.
                    if (originalWordData.Length >= NumberOfFields - 1)
                    {
                        String originalWord = originalWordData[2];
                        if (originalWord.Length > 0)
                        {
                            if (Regex.IsMatch(originalWord, Configuration.allowedCharacters))
                            {
                                aWordData.Letters = originalWord;

                                // Check that the 3rd field is in the wordlist.
                                if (!aCrozzle.WordList.Contains(originalWord))
                                    Errors.Add(String.Format(WordDataErrors.MissingWordError, originalWord));
                            }
                            else
                                Errors.Add(String.Format(WordDataErrors.AlphabeticError, originalWord));
                        }
                    }
                }
            }

            aWordData.Valid = Errors.Count == 0;
            return (aWordData.Valid);
        }
        #endregion
    }
}