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
            const string Ascii = @"

                   `.:::`                        ```         ``                       
                 `:+ooooo.                       syo        /yy`                      
              `-/ooooooooo.            ``        ydy        +dd`          ```         
            `:+oooooooooooo.        -+syyso:ss:  ydy  `/so: +dd`  -ss/`./ssyso:oso    
         `./oooooooooooooooo.     `ohho:-:+ydd+  ydy`:yho-  +dd`-shy: /hhs/-:/yhdy    
       `:+ooooooooooooooooooo-    /dd:     .hd+  ydhshh:    +ddohd+` .hdo     `ydy    
     ./ooooooooooossyhhhhddddh-   /dd:     .hd+  ydhsyhs.   +ddyshy: .hdo     `ydy    
  `-+ooooooooosyhhhdddddddddddh:  `ohho:-:+hdds- ydy``/hh+` +dd. :yho./hhs/-:/yhdh-`  
`/ooooooooosyhhhdddddddddddddddh.   -+syyso:/sys oso   .os+`/ss`  `+so-./syyys/-syy.  
/oooooooosyhhhddddddddhhdddddddh:      ```    ``                          ```    ``   
-oooooo/:-.```       ``.-ddhdddh`                                                     
 `-::-`                   `-::-` 

";
            var lightColor = Color.FromArgb(47, 171, 223);
            var darkColor = Color.FromArgb(25, 118, 186);

            var styleSheet = new StyleSheet(lightColor);
            styleSheet.AddStyle(@"yhhh[a-z]*[-:.]", darkColor);
            styleSheet.AddStyle(@":-.```", darkColor);
            styleSheet.AddStyle(@"``.-ddhdddh`", darkColor);
            styleSheet.AddStyle(@"`-::-`", darkColor);
            Console.WriteLineStyled(Ascii, styleSheet);
        }
    }
}