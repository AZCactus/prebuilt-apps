//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Text.RegularExpressions;

namespace FieldService.Android.Utilities {
    public static class Extensions {
        private static Regex _enumRegex = new Regex (@"[^\w\d_]");

        public static object ToEnum (Type enumType, string value)
        {
            try {
                if (!string.IsNullOrEmpty (value)) {
                    return Enum.Parse (enumType, _enumRegex.Replace (value, string.Empty), true);
                } else {
                    return Activator.CreateInstance (enumType);
                }
            } catch {
                return Activator.CreateInstance (enumType);
            }
        }

        public static int ToIntE6 (this float value)
        {
            return (int)(value * 1000000);
        }

        public static int ToInt (this string s)
        {
            var value = 0;
            int.TryParse (s, out value);
            return value;
        }
    }
}