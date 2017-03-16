using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace Akka.Cluster.Management.Cli.Utils
{
    public static class CommandLineApplicationExtensions
    {
        public static Task DisplayAsciiArtAsync(this CommandLineApplication target)
        {
            const string Ascii = @"
                                '''    
                             oo??i?oe                                   o'*             *?l                                 
                           'ii*oooioiv                                 '6w4+           +LuU+                                
                         i?i'veooio*ovo                                +wan+           +Ls7'                                
                      ''osi?o'**ooe'''so                 1i'           'ws6'           1n6h+               +'1'             
                    ooi'eoooo'o*oeoioi'oo            '7nnUUUUhVoeV76   1[6u1    ?wLU'  'Lu7+    iLUV'   *V[4hL[Lh'oUnn      
                  'loe'o?'ooiiivoiioo'oes'          ?Unhv'   'auuu4?   '7en+  '7nwe    'hL6'  'nL7s   lVhV4'   iv[nLa6      
                i*si*?i?'iooi*veeio'v*vo?o'        s46l        14uV*   +nL6  w4ws      +[ww  6nu[    'ulVo        wl44      
             +oo?ooeei''ooo'?oooi?v*iooioov+       V44'         owa7   'n[na6n7e       '76[aUn7*     wnwv         1u7w      
           i??ooiooiooooeovvoe?os?77l7*waloo       n64o         i[wa   'uuVLu6nV?      'nulh4u6ll    lu4?         '4l[      
         ioloioi*ooioiio'oaw?6uVwnVw[7VVaL[[s      LULw        'asw[   +64uw  *7uw     +777?  oww6+  '7aLi        7Vwa      
       *?o'*oeeioeoo'oo?76[nawu4wnu[V[4u47[u47'     aVnne'   i76hw4w   'heu    ih6no   'nVu    iu6n?  lh467i   '?[UL77'     
    'sseoio*o?ioe''oo?wLL77[6luawVLu7un46nLu4Vu      '6nhLnunh[o'hhn7u ih67+     e[[n+ +Uu4'     LhLh   on4UV64u6v'7Lh67    
   ?oooo?eoiooo'ol6664wh[an6[[nn4lV64L4Vwl4LVaul         ''''      'i                '               +     ''''      i'+    
  oo?o'eov?ov*iLu7[nls7eso?w*[?6snhnuawu674u7u47                                                                            
  iov'o*ov?a??vio                  '*wnn4nVnanL'                                                                            
   'looo?oi+                           'wVu767*                                                                             
     '+                                  ''o                                                                              


";
            return target.Out.WriteAsync(Ascii);
        }
    }
}