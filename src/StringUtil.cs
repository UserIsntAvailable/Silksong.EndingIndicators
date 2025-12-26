/*
 * Copyright (c) 2025 UserIsntAvailable
 * Copyright (c) 2025 silksong-modding
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the European Union Public Licence (EUPL)
 * version 1.2, as published by the European Commission.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * European Union Public Licence for more details.
 *
 * You should have received a copy of the European Union Public Licence
 * along with this program. If not, see
 * https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12
 */

using System.Text;

namespace Silksong.EndingIndicators;

internal static class StringUtil
{
    internal static string UnCamelCase(this string self)
    {
        // Don't change if it already has spaces.
        if (self.Contains(' '))
            return self;

        StringBuilder sb = new();

        bool prevUpper = false;
        bool first = true;
        foreach (var ch in self)
        {
            if (first)
            {
                first = false;
                sb.Append(char.ToUpper(ch));
                prevUpper = char.IsUpper(ch);
                continue;
            }

            if (char.IsUpper(ch) && !prevUpper)
                sb.Append(' ');
            sb.Append(ch);
            prevUpper = char.IsUpper(ch);
        }

        return sb.ToString();
    }
}
