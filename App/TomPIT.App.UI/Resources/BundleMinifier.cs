/*
 * This is a refactored version of:
 * @version   : 2.5.0
 * @author    : Ext.NET, Inc. http://www.ext.net/
 * @date      : 2014-10-20
 * @copyright : Copyright (c) 2008-2014, Ext.NET, Inc. (http://www.ext.net/). All rights reserved.
 * @license   : See license.txt and http://www.ext.net/license/. 
 * @website   : http://www.ext.net/
 */

using System;
using System.IO;
using System.Text;

namespace TomPIT.App.Resources
{
   internal class BundleMinifier
   {
      private const int Eof = -1;
      private int CurrentCharacter { get; set; }
      private int NextCharacter { get; set; }
      private int LookAhead { get; set; } = Eof;

      public string Minify(string value)
      {
         if (string.IsNullOrWhiteSpace(value))
            return value;

         var result = new StringBuilder();

         using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value), false);
         using var reader = new StreamReader(stream);
         using var writer = new StringWriter(result);

         Minify(reader, writer);

         return result.ToString();
      }


      private void Minify(StreamReader reader, StringWriter writer)
      {
         CurrentCharacter = '\n';
         Action(reader, writer, 3);

         while (CurrentCharacter != Eof)
         {
            switch (CurrentCharacter)
            {
               case ' ':
                  if (IsAlphaNumeric(NextCharacter))
                     Action(reader, writer, 1);
                  else
                     Action(reader, writer, 2);

                  break;
               case '\n':
                  switch (NextCharacter)
                  {
                     case '{':
                     case '[':
                     case '(':
                     case '+':
                     case '-':
                        Action(reader, writer, 1);
                        break;
                     case ' ':
                        Action(reader, writer, 3);
                        break;
                     default:
                        if (IsAlphaNumeric(NextCharacter))
                           Action(reader, writer, 1);
                        else
                           Action(reader, writer, 2);
                        break;
                  }
                  break;
               default:
                  switch (NextCharacter)
                  {
                     case ' ':
                        if (IsAlphaNumeric(CurrentCharacter))
                           Action(reader, writer, 1);
                        else
                           Action(reader, writer, 3);

                        break;
                     case '\n':
                        switch (CurrentCharacter)
                        {
                           case '}':
                           case ']':
                           case ')':
                           case '+':
                           case '-':
                           case '"':
                           case '\'':
                              Action(reader, writer, 1);
                              break;
                           default:
                              if (IsAlphaNumeric(CurrentCharacter))
                                 Action(reader, writer, 1);
                              else
                                 Action(reader, writer, 3);
                              break;
                        }
                        break;
                     default:
                        Action(reader, writer, 1);
                        break;
                  }
                  break;
            }
         }
      }

      private void Action(StreamReader reader, StringWriter writer, int mode)
      {
         if (mode <= 1)
            Put(writer, CurrentCharacter);

         if (mode <= 2)
         {
            CurrentCharacter = NextCharacter;

            if (CurrentCharacter == '\'' || CurrentCharacter == '"')
            {
               for (; ; )
               {
                  Put(writer, CurrentCharacter);
                  CurrentCharacter = Get(reader);

                  if (CurrentCharacter == NextCharacter)
                     break;

                  if (CurrentCharacter <= '\n')
                     throw new Exception($"Minifier unterminated string literal: {CurrentCharacter}\n");

                  if (CurrentCharacter == '\\')
                  {
                     Put(writer, CurrentCharacter);
                     CurrentCharacter = Get(reader);
                  }
               }
            }
         }
         if (mode <= 3)
         {
            NextCharacter = Next(reader);

            if (NextCharacter == '/' && (CurrentCharacter == '(' || CurrentCharacter == ',' || CurrentCharacter == '=' ||
                                CurrentCharacter == '[' || CurrentCharacter == '!' || CurrentCharacter == ':' ||
                                CurrentCharacter == '&' || CurrentCharacter == '|' || CurrentCharacter == '?' ||
                                CurrentCharacter == '{' || CurrentCharacter == '}' || CurrentCharacter == ';' ||
                                CurrentCharacter == '\n'))
            {
               Put(writer, CurrentCharacter);
               Put(writer, NextCharacter);

               for (; ; )
               {
                  CurrentCharacter = Get(reader);

                  if (CurrentCharacter == '/')
                  {
                     break;
                  }
                  else if (CurrentCharacter == '\\')
                  {
                     Put(writer, CurrentCharacter);
                     CurrentCharacter = Get(reader);
                  }
                  else if (CurrentCharacter <= '\n')
                     throw new Exception(string.Format("Error: JSMIN unterminated Regular Expression literal : {0}.\n", CurrentCharacter));

                  Put(writer, CurrentCharacter);
               }

               NextCharacter = Next(reader);
            }
         }
      }

      private int Next(StreamReader reader)
      {
         var current = Get(reader);

         if (current == '/')
         {
            switch (Peek(reader))
            {
               case '/':
                  for (; ; )
                  {
                     current = Get(reader);

                     if (current <= '\n')
                        return current;
                  }
               case '*':
                  Get(reader);

                  for (; ; )
                  {
                     switch (Get(reader))
                     {
                        case '*':
                           {
                              if (Peek(reader) == '/')
                              {
                                 Get(reader);

                                 return ' ';
                              }
                              break;
                           }
                        case Eof:
                           throw new Exception("Error: JSMIN Unterminated comment.\n");
                     }
                  }
               default:
                  return current;
            }
         }

         return current;
      }

      private int Peek(StreamReader reader)
      {
         LookAhead = Get(reader);

         return LookAhead;
      }

      private int Get(StreamReader reader)
      {
         var current = LookAhead;
         LookAhead = Eof;

         if (current == Eof)
            current = reader.Read();

         if (current >= ' ' || current == '\n' || current == Eof)
            return current;

         if (current == '\r')
            return '\n';

         return ' ';
      }

      private void Put(StringWriter writer, int character)
      {
         writer.Write((char)character);
      }

      private bool IsAlphaNumeric(int character)
      {
         return ((character >= 'a' && character <= 'z')
            || (character >= '0' && character <= '9')
            || (character >= 'A' && character <= 'Z')
            || character == '_' || character == '$' || character == '\\' || character > 126);
      }
   }
}