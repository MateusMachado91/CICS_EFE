using System. Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind. Sequential, CharSet = CharSet.Ansi)]
public struct Book

{
    [MarshalAs(UnmanagedType. ByValArray, SizeConst = 1)]
    public char[] VERSAOC01;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public char[] CODRETORNOC02;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public char[] LOGC01;
    [MarshalAs(UnmanagedType. ByValArray, SizeConst = 2)]
    public char[] PEDIDOC02;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public char[] SISTEMAC03;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public char[] PERMISSAOC03;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public char[] USUARIOC15;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public char[] SENHAC15;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public char[] TIPOARQUIVOC01;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public char[] SERVIDORREMOTOC20;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
    public char[] ARQUIVOLOCALC260;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
    public char[] ARQUIVOREMOTOC260;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public char[] NUMEROJOBC05;
}

class Program
{
    [DllImport("pxuotrxw.dll", CallingConvention = CallingConvention. StdCall)]
    private static extern int pxuotrxw(ref BookPXUOTRXW informacoes, StringBuilder msgRetornoC201);

    static void Main()
    {
        Console.WriteLine("=== TESTE SEGURO DE AUTORIZAÇÃO (não cria arquivos) ===\n");

        string[] sistemas = { "PYB", "MQM" }; // Testar vários
        
        foreach (var sistema in sistemas)
        {
            TestarAutorizacao(sistema);
            Console.WriteLine();
        }

        Console.WriteLine("Pressione Enter para sair...");
        Console.ReadLine();
    }

    static void TestarAutorizacao(string sistema)
    {
        try
        {
            Console.WriteLine($"🔍 Sistema: {sistema}");
            
            // ⭐ TRUQUE:  Usar arquivo que NÃO EXISTE
            // A DLL valida autorização ANTES de tentar transferir
            string arquivoInexistente = @"C:\arquivo_que_nao_existe_teste_123456789.txt";

            var info = new BookPXUOTRXW
            {
                VERSAOC01 = "5". PadRight(1).ToCharArray(),
                CODRETORNOC02 = "00".PadRight(2).ToCharArray(),
                LOGC01 = "S".PadRight(1).ToCharArray(),
                PEDIDOC02 = "01".PadRight(2).ToCharArray(), // 01 = Copiar
                SISTEMAC03 = sistema.PadRight(3).ToCharArray(),
                PERMISSAOC03 = "664". PadRight(3).ToCharArray(),
                USUARIOC15 = " ".PadRight(15).ToCharArray(),
                SENHAC15 = " ".PadRight(15).ToCharArray(),
                TIPOARQUIVOC01 = "A".PadRight(1).ToCharArray(),
                SERVIDORREMOTOC20 = "ibm".PadRight(20).ToCharArray(),
                ARQUIVOLOCALC260 = arquivoInexistente.PadRight(260).ToCharArray(),
                ARQUIVOREMOTOC260 = " ".PadRight(260).ToCharArray(),
                NUMEROJOBC05 = " ".PadRight(5).ToCharArray()
            };

            var mensagem = new StringBuilder(201);
            int retorno = pxuotrxw(ref info, mensagem);
            string codigoRetorno = new string(info.CODRETORNOC02).Trim();
            string msg = mensagem.ToString().Trim();

            // Interpretar resultado
            if (codigoRetorno == "14")
            {
                Console.ForegroundColor = ConsoleColor. Red;
                Console.WriteLine($"   ❌ NÃO AUTORIZADO");
                Console.WriteLine($"   Código: 14 - Servidor/Sistema não cadastrado");
            }
            else if (codigoRetorno == "03")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ❌ NÃO AUTORIZADO");
                Console.WriteLine($"   Código: 03 - Transferência não autorizada");
            }
            else if (codigoRetorno == "90" || codigoRetorno == "82")
            {
                // Erro 90/82 = arquivo não existe (esperado!)
                // MAS se chegou aqui, a AUTORIZAÇÃO foi validada! 
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ✅ AUTORIZADO!");
                Console.WriteLine($"   (Erro {codigoRetorno} é esperado - arquivo de teste não existe)");
            }
            else if (codigoRetorno == "00")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console. WriteLine($"   ✅ AUTORIZADO E CONECTADO!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   ⚠️ Código: {codigoRetorno}");
            }
            
            Console.WriteLine($"   Mensagem: {msg}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor. Red;
            Console.WriteLine($"   ❌ Erro:  {ex.Message}");
            Console.ResetColor();
        }
    }
}