using DllConsultaCNPJ;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsForms
{
    public partial class frmFormulario : Form
    {
        
        public frmFormulario() // ALTEREI O NOME DO FORMULARIO PARA NOME COMUM, PARA NAO CONFUNDIR COM A CLASSE ConsultaCNPJReceita
        {
            InitializeComponent();
        }

        private void frmConsultaCNPJ_Load(object sender, EventArgs e)
        {
            CarregaCaptcha(); 
        }

        private void btConsultar_Click(object sender, EventArgs e)
        {
            ConsultaCNPJ();
        }

        private void btTrocarImagem_Click(object sender, EventArgs e)
        {
            CarregaCaptcha();
        }


        private async void CarregaCaptcha() // METODO ASYNC (ASSINCRONO) // PARA NAO TRAVAR O FORM QUANDO DEMORAR
        {
            LimpaCamposCaptcha();

            // EM MEUS TESTES, APÓS ALGUMAS CONSULTAS A RECEITA BLOQUEOU O IP, MESMO ACESSANDO PELO SITE DA RECEITA. NAO FUNCIONOU ATÉ REINICIAR A INTERNET.
            // DESSA FORMA CONSEGUI IMPLEMENTAR ROTINAS DE QUANDO O SERVIÇO FICA LENTO OU É BLOQUEADO

            // PARA BLOQUEAR O SEU IP NA RECEITA, BASTA CLICAR DIVERSAS VEZES NO BOTAO TROCAR IMAGEM. A CONSULTA VAI DEMORAR PARA DAR RETORNO DE ERRO DEVIDO AO BLOQUEIO.
            // PORÉM, CONFORME O MODO ASSINCRONO IMPLEMENTADO ABAIXO, O SISTEMA NÃO FICARÁ TRAVADO ENQUANTO A CONSULTA NAO RETORNA NADA.

            await Task.Run(() => // EXECUTA O MÉTODO EM UMA NOVA TASK, NAO TRAVANDO A TELA AO INICIAR QUANDO O SERVIÇO NAO ESTA DISPONIVEL OU ESTA MUITO LENTO
            {
                BloqueiaBotaoTrocaImagem(true); // BLOQUEIA O BOTAO E EXIBE MENSAGEM CARREGANDO NO BOTAO

                // SIMPLESMENTE CARREGA A IMAGEM NO PICTUREBOX INFORMADO
                if (ConsultaCNPJReceita.GetCaptcha(picLetras) == false)
                {
                    MessageBox.Show(ConsultaCNPJReceita.Mensagem); // MENSAGEM SE TIVER ERRO
                }

                BloqueiaBotaoTrocaImagem(false); // RETORNA O BOTAO AO ESTADO ORIGINAL
            });
        }

        private void LimpaCamposCaptcha()
        {
            Invoke((MethodInvoker)(() =>
            {
                txtLetras.Text = "";
                txtLetras.Focus();
            }));
        }

        private void BloqueiaBotaoTrocaImagem(bool Bloquear)
        {
            // COMO O ESSE METODO É CHAMADO DENTRO DE UMA TASK (await Task.Run(() =>) PRECISAMOS APENAS CHAMAR INVOKE() PARA MANIPULAR COMPONENTES DO FORM.
            // SEM INVOKE() OCORRERÁ ERRO
            Invoke((MethodInvoker)(() => {
                if (Bloquear == true)
                {
                    btTrocarImagem.Enabled = false;
                    btTrocarImagem.Text = "Carregando Imagem...";
                }else
                {
                    btTrocarImagem.Enabled = true;
                    btTrocarImagem.Text = "Trocar Imagem";
                }
            }));
        }

        private void BloqueiaBotaoConsultaCNPJ(bool Bloquear)
        {
            // COMO O ESSE METODO É CHAMADO DENTRO DE UMA TASK (await Task.Run(() =>) PRECISAMOS APENAS CHAMAR INVOKE() PARA MANIPULAR COMPONENTES DO FORM.
            // SEM INVOKE() OCORRERÁ ERRO
            Invoke((MethodInvoker)(() => {
                if (Bloquear == true)
                {
                    btConsultar.Enabled = false;
                    btConsultar.Text = "Carregando...";
                }
                else
                {
                    btConsultar.Enabled = true;
                    btConsultar.Text = "Consultar CNPJ";
                }
            }));
        }

        private async void ConsultaCNPJ()
        {
            // VERIFICO AQUI OS CAMPOS PARA NAO RECARREGAR O CAPCHA NOVAMENTE
            // A VALIDACAO É FEITA TAMBEM DENTRO DE ConsultaCNPJReceita.Consulta(); PARA CASO O USUARIO NAO IMPLENTE ISSO NO FORMULARIO
            if (ConsultaCNPJReceita.ValidaCampos(txtCNPJ.Text, txtLetras.Text) == false)
            {
                MessageBox.Show(ConsultaCNPJReceita.Mensagem);
                return;
            }


            await Task.Run(() => // EXECUTA O MÉTODO EM UMA NOVA TASK, NAO TRAVANDO A TELA QUANDO O SERVIÇO NAO ESTA DISPONIVEL OU ESTA MUITO LENTO
            {
                BloqueiaBotaoConsultaCNPJ(true); //BLOQUEIA O BOTAO E EXIBE MENSAGEM CARREGANDO NO BOTAO

                if (ConsultaCNPJReceita.Consulta(txtCNPJ.Text, txtLetras.Text))
                    CarregaDadosNoFormulario();
                else
                    MessageBox.Show(ConsultaCNPJReceita.Mensagem);

                BloqueiaBotaoConsultaCNPJ(false); // RETORNA O BOTAO AO NORMAL
                CarregaCaptcha(); // APÓS UMA CONSULTA, RECARREGA CAPTCHA NOVAMENTE.
            });
        }


        private void CarregaDadosNoFormulario()
        {
            // COMO ESSE METODO É CHAMADO DENTRO DE UMA NOVA TASK (await Task.Run(() =>) PRECISAMOS CHAMAR INVOKE() PARA MEXER COM COMPONENTES DO FORM.
            // SEM INVOKE() OCORRERÁ ERRO
            Invoke((MethodInvoker)(() =>
            {
                Empresa empresaConsultada;

                empresaConsultada = ConsultaCNPJReceita.Empresa;

                txtRazao.Text = empresaConsultada.RazaoSocial;
                txtFantasia.Text = empresaConsultada.NomeFantasia;
                txtAtividadeEconomeica.Text = empresaConsultada.AtividadeEconomicaPrimaria;
                txtAtividadeEconomicaSecundaria.Text = empresaConsultada.AtividadeEconomicaSecundaria;
                txtNaturezaJuridica.Text = empresaConsultada.NaturezaJuridica;

                //Endereco
                txtLogradouro.Text = empresaConsultada.Endereco;
                txtNumero.Text = empresaConsultada.Numero;
                txtComplemento.Text = empresaConsultada.Complemento;
                txtBairro.Text = empresaConsultada.Bairro;
                txtCidade.Text = empresaConsultada.Cidade;
                txtUF.Text = empresaConsultada.UF;
                txtCEP.Text = empresaConsultada.CEP;

                //Contato
                txtEmail.Text = empresaConsultada.Email;
                txtTelefone.Text = empresaConsultada.Telefones; // pode ser mais de um

                //Situações
                txtSituacaoCadastral.Text = empresaConsultada.SituacaoCadastral;
                txtDataSituacaoCadastral.Text = empresaConsultada.DataSituacaoCadastral;
                txtMotivoSituacaoCadastral.Text = empresaConsultada.MotivoSituacaoCadastral;

                txtMatrizFilial.Text = empresaConsultada.MatrizFilial;
                txtDataAbertura.Text = empresaConsultada.DataAbertura;
                txtResponsavel.Text = empresaConsultada.EnteFederativoResponsavel;
                txtSituacaoEspecial.Text = empresaConsultada.SituacaoEspecial;
                txtDataSituacaoEspecial.Text = empresaConsultada.DataSituacaoEspecial;
                txtCNAE.Text = empresaConsultada.Cnae;

            }));
        }
    }
}
