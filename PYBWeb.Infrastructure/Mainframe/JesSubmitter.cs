using System.Runtime.InteropServices;
using System.Text;

namespace PYBWeb.Infrastructure.Mainframe
{
    public class JesSubmitter
    {
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct BookPXUOTRXW
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public char[] VERSAOC01;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] CODRETORNOC02;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public char[] LOGC01;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
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
   
        [DllImport("pxuotrxw.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int pxuotrxw(ref BookPXUOTRXW informacoes, StringBuilder msgRetornoC201);

        /// <summary>
        /// Submete um arquivo JCL ao mainframe JES2
        /// O JCL já deve estar completo com todos os comentários e configurações
        /// </summary>

        public static (bool sucesso, string mensagem, string codigoRetorno, string numeroJob) SubmeterJCL(
            string caminhoJclLocal)
        {
            try
            {
                if (! File.Exists(caminhoJclLocal))
                {
                    return (false, "❌ Arquivo JCL não encontrado.", "82", "");
                }

                var informacoes = new BookPXUOTRXW
                {
                    VERSAOC01 = "2". PadRight(1).ToCharArray(), //MUDAMOS PARA VERSÃO 2(NÃO PRECISA DO .FMT) VERSÃO 5 PRECISA DO .FMT
                    CODRETORNOC02 = "00".PadRight(2).ToCharArray(),
                    LOGC01 = "S".PadRight(1).ToCharArray(),
                    PEDIDOC02 = "05".PadRight(2).ToCharArray(), // 05 = Submit JCL
                    SISTEMAC03 = "PYB".PadRight(3).ToCharArray(), //Sistema PYB
                    PERMISSAOC03 = "000".PadRight(3).ToCharArray(), //MUDAMOS PARA 000 IGUAL NO SISTEMA LEGADO(DOCUMENTAÇÃO PEDIA 664 PARA VERSÃO 5)
                    USUARIOC15 = " ".PadRight(15).ToCharArray(), //Em Branco
                    SENHAC15 = " ".PadRight(15).ToCharArray(), // Em Branco
                    TIPOARQUIVOC01 = "A".PadRight(1).ToCharArray(), //ASCII
                    SERVIDORREMOTOC20 = "ibm".PadRight(20).ToCharArray(), // Mainframe
                    ARQUIVOLOCALC260 = caminhoJclLocal.PadRight(260).ToCharArray(),
                    ARQUIVOREMOTOC260 = " ".PadRight(260).ToCharArray(),
                    NUMEROJOBC05 = " ".PadRight(5).ToCharArray()
                };

                var mensagemRetorno = new StringBuilder(201);
                // 3. Chamar DLL
                int retorno = pxuotrxw(ref informacoes, mensagemRetorno);

                string codigoRetorno = new string(informacoes.CODRETORNOC02).Trim();
                string mensagem = mensagemRetorno.ToString().Trim();
                string numeroJob = new string(informacoes.NUMEROJOBC05).Trim();

                // 4. Processar resultado
                if (retorno == 0 && codigoRetorno == "00")
                {
                    string msgSucesso = "✅ JCL submetido com sucesso!";
                    if (! string.IsNullOrEmpty(numeroJob))
                        msgSucesso += $"\n📋 JOB: {numeroJob}";
                    if (!string.IsNullOrEmpty(mensagem))
                        msgSucesso += $"\n{mensagem}";

                    return (true, msgSucesso, codigoRetorno, numeroJob);
                }
                else
                {
                    string msgErro = $"❌ Falha ao submeter JCL\nCódigo:  {codigoRetorno}";
                    msgErro += ObterDescricaoErro(codigoRetorno);
                    
                    if (!string.IsNullOrEmpty(mensagem))
                        msgErro += $"\n\n{mensagem}";

                    // Log adicional para erro "01"

                    return (false, msgErro, codigoRetorno, "");
                }
            }
            catch (DllNotFoundException)
            {
                return (false, "❌ DLL pxuotrxw. dll não encontrada.", "99", "");
            }
            catch (BadImageFormatException)
            {
                return (false, "❌ Incompatibilidade 32-bit/64-bit.\nVerifique <PlatformTarget>x86</PlatformTarget>", "98", "");
            }
            catch (Exception ex)
            {
                return (false, $"❌ Erro:  {ex.Message}", "97", "");
            }
        }

        private static string ObterDescricaoErro(string codigo)
        {
            return codigo switch
            {
                "00" => "",
                "01" => "\nFalha genérica.  Consulte o log.",
                "03" => "\nTransferência não autorizada (tabela PXQTTRL).",
                "04" => "\nArquivo em uso por outro processo.",
                "11" => "\nTimeout do FTP.",
                "14" => "\nServidor/diretório não cadastrado.",
                "16" => "\nEspaço insuficiente no mainframe.",
                "18" => "\nServiço JES indisponível.",
                "82" => "\nFalha ao ler o arquivo JCL local.",
                "90" => "\nArquivo JCL inacessível ou sem permissão.",
                "A0" => "\nArquivo JCL vazio (0 bytes).",
                _ => $"\nErro desconhecido ({codigo})."
            };
        }
    }
}