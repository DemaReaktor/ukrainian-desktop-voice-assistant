using System;
using System.Data;
using System.Linq;
using mphweb.Models;
using mphdict;
using mphdict.Models.morph;
using System.IO;
using Microsoft.EntityFrameworkCore;
using uSofTrod.generalTypes.Models;
using NAudio.Wave;
using System.ComponentModel;
using static Speech2.SearchWordCommand;
using System.Collections.Generic;

namespace Speech2
{
    class WordVariantsCommand : ICommand
    {
        DataB context;
        public EventHandler OnGetWords;

        public WordVariantsCommand()
        {
            this.context = MphDataBaseConstructor.DataBase;
        }

        [Description("скажи слова і слово яке шукаєш")]
        public bool Check(string text) => text.Split(" ")[0].Equals("слова");
        [Description("виводить опис слова, причому всі варіанти")]
        public void Execute(Assistant assistant, string text)
        {
            string word = text.Remove(0, 6);
            OnGetWords?.Invoke(this, new WordEventArgs() { Uri = GetWordsUri(word) });
            assistant.Speak("слова " + word);
        }
        //показує розбір слова
        public Uri GetWordsUri(string word)
        {
            string templ, str = "";

            using (var stream = new StreamWriter("words.html"))
            {
                stream.WriteLine("<!DOCTYPE html>");
                stream.WriteLine("<html>");
                stream.WriteLine("<meta charset=\"UTF-8\">");
                stream.WriteLine("<body>");

                Dictionary<int,word_param> words = new Dictionary<int,word_param>();
                foreach (var element in MphDataBaseConstructor.Bases)
                    if (word.StartsWith(element.Value.ToLower()))
                    {
                        element.Key.indents.flexes = (from c in context.flexes.AsNoTracking() where (c.type == element.Key.type /*&& (c.field2 > 0)*/) orderby c.field2, c.id select c).ToList();
                        foreach (var flex in element.Key.indents.flexes)
                            if ((element.Value + flex.flex).ToLower() == word)
                            {
                                words[element.Key.nom_old] = element.Key;
                                break;
                            }
                    }

                foreach (var item in words.Values)
                {
                    templ = "";
                    item.parts = (from c in context.parts.AsNoTracking() where c.id == item.part select c).First();
                    item.accents_class = (from c in context.accents_class.AsNoTracking() select c).First();
                    item.accents_class.accents = (from c in context.accent.AsNoTracking() where c.accent_type == item.accent select c).ToArray();

                    if (item.field7 != null)
                        str = item.field7.Replace("<", "&#60;").Replace(">", "&#62;");
                    str += "<br>";
                    if (item.field6 != null)
                        str += item.field6.Replace("<", "&#60;").Replace(">", "&#62;");
                    str += "<br>";
                    if (item.indents.comment != null)
                        str += item.indents.comment.Replace("<", "&#3C;").Replace(">", "&#3E;");
                    string rdv = string.Empty;
                    templ += mphEntry.generateTempl(item, out rdv);

                    templ = templ.Replace("[WORD]", item.reestr.Replace("\"", ""));
                    templ = templ.Replace("[gram]", item.parts.com);
                    str = str.Replace("$", rdv);
                    templ = templ.Replace("*[text]", str);
                    if (item.field5 != null)
                        templ = templ.Replace("[(sem comment)]", item.field5.Replace("<", "&#60;").Replace(">", "&#62;"));
                    else templ = templ.Replace("[(sem comment)]", "");

                    stream.WriteLine(templ);
                    stream.WriteLine("<p style=\"color:blue;\">|||||||||||||||||||||||||||||||||</p>");
                }
                stream.WriteLine("</body>");
                stream.WriteLine("</html>");
            }

            return new Uri("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\words.html");
        }
    }
}
