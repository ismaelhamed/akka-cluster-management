using System.Drawing;
using Colorful;
using Microsoft.Extensions.CommandLineUtils;
using Console = Colorful.Console;

namespace Akka.Cluster.Management.Cli.Utils
{
    public static class CommandLineApplicationExtensions
    {
        public static void DisplayAsciiArt(this CommandLineApplication target)
        {
            const string ascii = @"
 
                    `.:::`                        ```         ``                       
                  `:+ooooo.                       syo        /yy`                      
               `-/ooooooooo.            ``        ydy        +dd`          ```         
             `:+oooooooooooo.        -+syyso:ss:  ydy  `/so: +dd`  -ss/ -+syyso:ss:    
          `./oooooooooooooooo.     `ohho:-:+ydd+  ydy`:yho-  +dd`-shy: /hhs/-:/yhdy    
        `:+ooooooooooooooooooo-    /dd:     .hd+  ydhshh:    +ddohd+  .hdo     `ydy    
      ./oooooooooooss{0}   /dd:     .hd+  ydhsyhs.   +ddyshy: .hdo     `ydy    
   `-+ooooooooos{1}  `ohho:-:+hdds- ydy``/hh+` +dd. :yho ohho:-:+hdds-  
 `/ooooooooos{2}   -+syyso:/sys oso   .os+ /ss`  `+so `/syyys/-sys  
 /oooooooos{3}      ```    ``                          ```    ``   
 -oooooo/{4}       {5}                                                     
  `-::-`                   {6} 

";
            var lightColor = Color.FromArgb(47, 171, 223);
            var darkColor = Color.FromArgb(25, 118, 186);
            var fruits = new[]
            {
                new Formatter("yhhhhddddh-", darkColor),
                new Formatter("yhhhdddddddddddh:", darkColor),
                new Formatter("yhhhdddddddddddddddh.", darkColor),
                new Formatter("yhhhddddddddhhdddddddh:", darkColor),
                new Formatter(":-.```", darkColor),
                new Formatter("``.-ddhdddh`", darkColor),
                new Formatter("`-::-`", darkColor)
            };
            Console.WriteLineFormatted(ascii, lightColor, fruits);
        }
    }
}