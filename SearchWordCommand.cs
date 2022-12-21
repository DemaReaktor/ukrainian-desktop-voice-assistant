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

namespace Speech2
{
     class SearchWordCommand : ICommand
    {
        public class WordEventArgs : EventArgs { public Uri Uri; }
        DataB context;
        public EventHandler OnGetWord;

        public SearchWordCommand()
        {
           context = MphDataBaseConstructor.DataBase;
        }

        [Description("скажи знайти і слово яке шукаєш")]
        public bool Check(string text) => text.Split(" ")[0].Equals("знайти");
        [Description("виводить опис слова")]
        public void Execute(Assistant assistant, string text)
        {
            string word = text.Remove(0, 7);
            OnGetWord?.Invoke(this, new WordEventArgs() { Uri = GetWordUri(word)});
            assistant.Speak("слово " + word);
        }
        //показує розбір слова
        public Uri GetWordUri(string word)
        {
            string templ = "", str = "";
            alphadigit[] al = (from c in context.alphadigits orderby c.digit, c.ls select c).ToArray();
            var q = (from c in context.words_list select c);
            var w = sharedTypes.atod(word, al);
            q = q.Where(c => EF.Functions.Like(c.digit, w));
            q = q.OrderBy(c => c.digit).ThenBy(c => c.field2);
            q = q.Skip((from c in q where w.CompareTo(c.digit) > 0 select c).Count() - 1).Take(1);
            word_param item = q.First();
            item.parts = (from c in context.parts.AsNoTracking() where c.id == item.part select c).First();
            item.indents = (from c in context.indents.AsNoTracking() where c.type == item.type select c).First();
            item.indents.flexes = (from c in context.flexes.AsNoTracking() where (c.type == item.type && (c.field2 > 0)) orderby c.field2, c.id select c).ToList();
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
            using (var stream = new StreamWriter("word.html"))
            {
                stream.WriteLine("<!DOCTYPE html>");
                stream.WriteLine("<html>");
                stream.WriteLine("<meta charset=\"UTF-8\">");
                stream.WriteLine("<body>");
                stream.WriteLine(templ);
                stream.WriteLine("</body>");
                stream.WriteLine("</html>");
            }
            return new Uri("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\word.html");
        }
    }
}
