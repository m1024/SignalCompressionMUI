using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace SignalCompressionMUI.Models.Algorithms
{
    public enum WaveletType
    {
        [Description("Хаар (D2)")]
        Haar,
        [Description("Добеши D4")]
        D4,
        [Description("Добеши D6")]
        D6,
        [Description("Добеши D8")]
        D8,
        [Description("Добеши D10")]
        D10,
        [Description("Добеши D12")]
        D12,
        [Description("Добеши D14")]
        D14,
        [Description("Добеши D16")]
        D16,
        [Description("Добеши D18")]
        D18,
        [Description("Добеши D20")]
        D20,
        [Description("Симмлет S2")]
        Sym2,
        [Description("Симмлет S3")]
        Sym3,
        [Description("Симмлет S4")]
        Sym4,
        [Description("Симмлет S5")]
        Sym5,
        [Description("Симмлет S6")]
        Sym6,
        [Description("Симмлет S7")]
        Sym7,
        [Description("Симмлет S8")]
        Sym8,
        [Description("Симмлет S9")]
        Sym9,
        [Description("Симмлет S10")]
        Sym10,
        [Description("Койфлет С1")]
        Coif1,
        [Description("Койфлет С2")]
        Coif2,
        [Description("Койфлет С3")]
        Coif3,
        [Description("Койфлет С4")]
        Coif4,
        [Description("Койфлет С5")]
        Coif5,
        [Description("Мейер")]
        Meyer,
        [Description("Биорт. 1.1")]
        Biorth1_1,
        [Description("Биорт. 1.3")]
        Biorth1_3,
        [Description("Биорт. 1.5")]
        Biorth1_5,
        [Description("Биорт. 2.2")]
        Biorth2_2,
        [Description("Биорт. 2.4")]
        Biorth2_4,
        [Description("Биорт. 2.6")]
        Biorth2_6,
        [Description("Биорт. 2.8")]
        Biorth2_8,
        [Description("Биорт. 3.1")]
        Biorth3_1,
        [Description("Биорт. 3.3")]
        Biorth3_3,
        [Description("Биорт. 3.5")]
        Biorth3_5,
        [Description("Биорт. 3.7")]
        Biorth3_7,
        [Description("Биорт. 3.9")]
        Biorth3_9,
        [Description("Биорт. 4.4")]
        Biorth4_4,
        [Description("Биорт. 5.5")]
        Biorth5_5,
        [Description("Биорт. 6.8")]
        Biorth6_8,
    }

    public static class AlgorithmWv
    {
        //использовать нормированные или не нормированные?
        #region Ортогональные нормированные коэффициенты Добеши

        private static readonly double[] Haar = {1.0, 1.0};

        private static readonly double[] D4 = {0.6830127, 1.1830127, 0.3169873, -0.1830127};

        private static readonly double[] D6 = {0.47046721, 1.14111692, 0.650365, -0.19093442, -0.12083221, 0.0498175};

        private static readonly double[] D8 = {0.32580343, 1.01094572, 0.8922014, -0.03957503, -0.26450717, 0.0436163, 0.0465036, -0.01498699};

        private static readonly double[] D10 = {0.22641898, 0.85394354, 1.02432694, 0.19576696, -0.34265671, -0.04560113, 0.10970265, -0.00882680, -0.01779187, 4.71742793e-3};

        private static readonly double[] D12 = { 0.15774243, 0.69950381, 1.06226376, 0.44583132, -0.31998660, -0.18351806, 0.13788809, 0.03892321, -0.04466375, 7.83251152e-4, 6.75606236e-3, -1.52353381e-3 };

        private static readonly double[] D14 = { 0.11009943, 0.56079128, 1.03114849, 0.66437248, -0.20351382, -0.31683501, 0.1008467, 0.11400345, -0.05378245, -0.02343994, 0.01774979, 6.07514995e-4, -2.54790472e-3, 5.00226853e-4 };

        private static readonly double[] D16 = { 0.07695562, 0.44246725, 0.95548615, 0.82781653, -0.02238574, -0.40165863, 6.68194092e-4, 0.18207636, -0.02456390, -0.06235021, 0.01977216, 0.01236884, -6.88771926e-3, -5.54004549e-4, 9.55229711e-4, -1.66137261e-4 };

        private static readonly double[] D18 = { 0.05385035, 0.34483430, 0.34483430, 0.92954571, 0.18836955, -0.41475176, -0.13695355, 0.21006834, 0.043452675, -0.09564726, 3.54892813e-4, 0.03162417, -6.67962023e-3, -6.05496058e-3, 2.61296728e-3, 3.25814671e-4, -3.56329759e-4, 5.5645514e-5 };

        private static readonly double[] D20 = { 0.03771716, 0.26612218, 0.74557507, 0.97362811, 0.39763774, -0.35333620, -0.27710988, 0.18012745, 0.13160299, -0.10096657, -0.04165925, 0.04696981, 5.10043697e-3, -0.01517900, 1.97332536e-3, 2.81768659e-3, -9.69947840e-4, -1.64709006e-4, 1.32354367e-4, -1.875841e-5 };

        #endregion

        #region Симмлеты

        private static readonly double[] Sym2 =
        {
            -0.12940952255092145,
            0.22414386804185735,
            0.836516303737469,
            0.48296291314469025,
        };

        private static readonly double[] Sym3 =
        {
            0.035226291882100656,
            -0.08544127388224149,
            -0.13501102001039084,
            0.4598775021193313,
            0.8068915093133388,
            0.3326705529509569
        };

        private static readonly double[] Sym4 =
        {
            -0.07576571478927333,
            -0.02963552764599851,
            0.49761866763201545,
            0.8037387518059161,
            0.29785779560527736,
            -0.09921954357684722,
            -0.012603967262037833,
            0.0322231006040427
        };


        private static readonly double[] Sym5 =
        {
            0.027333068345077982,
            0.029519490925774643,
            -0.039134249302383094,
            0.1993975339773936,
            0.7234076904024206,
            0.6339789634582119,
            0.01660210576452232,
            -0.17532808990845047,
            -0.021101834024758855,
            0.019538882735286728
        };


        private static readonly double[] Sym6 =
        {
            0.015404109327027373,
            0.0034907120842174702,
            -0.11799011114819057,
            -0.048311742585633,
            0.4910559419267466,
            0.787641141030194,
            0.3379294217276218,
            -0.07263752278646252,
            -0.021060292512300564,
            0.04472490177066578,
            0.0017677118642428036,
            -0.007800708325034148
        };

        private static readonly double[] Sym7 =
        {
            0.002681814568257878,
            -0.0010473848886829163,
            -0.01263630340325193,
            0.03051551316596357,
            0.0678926935013727,
            -0.049552834937127255,
            0.017441255086855827,
            0.5361019170917628,
            0.767764317003164,
            0.2886296317515146,
            -0.14004724044296152,
            -0.10780823770381774,
            0.004010244871533663,
            0.010268176708511255
        };

        private static readonly double[] Sym8 =
        {
            -0.0033824159510061256,
            -0.0005421323317911481,
            0.03169508781149298,
            0.007607487324917605,
            -0.1432942383508097,
            -0.061273359067658524,
            0.4813596512583722,
            0.7771857517005235,
            0.3644418948353314,
            -0.05194583810770904,
            -0.027219029917056003,
            0.049137179673607506,
            0.003808752013890615,
            -0.01495225833704823,
            -0.0003029205147213668,
            0.0018899503327594609
        };

        private static readonly double[] Sym9 =
        {
            0.0014009155259146807,
            0.0006197808889855868,
            -0.013271967781817119,
            -0.01152821020767923,
            0.03022487885827568,
            0.0005834627461258068,
            -0.05456895843083407,
            0.238760914607303,
            0.717897082764412,
            0.6173384491409358,
            0.035272488035271894,
            -0.19155083129728512,
            -0.018233770779395985,
            0.06207778930288603,
            0.008859267493400484,
            -0.010264064027633142,
            -0.0004731544986800831,
            0.0010694900329086053
        };

        private static readonly double[] Sym10 =
        {
            0.0007701598091144901,
            9.563267072289475e-05,
            -0.008641299277022422,
            -0.0014653825813050513,
            0.0459272392310922,
            0.011609893903711381,
            -0.15949427888491757,
            -0.07088053578324385,
            0.47169066693843925,
            0.7695100370211071,
            0.38382676106708546,
            -0.03553674047381755,
            -0.0319900568824278,
            0.04999497207737669,
            0.005764912033581909,
            -0.02035493981231129,
            -0.0008043589320165449,
            0.004593173585311828,
            5.7036083618494284e-05,
            -0.0004593294210046588
        };

        #endregion

        #region Койфлеты

        private static readonly double[] Coif1 =
        {
            -0.01565572813546454,
            -0.0727326195128539,
            0.38486484686420286,
            0.8525720202122554,
            0.3378976624578092,
            -0.0727326195128539,
        };

        private static readonly double[] Coif2 =
        {
            -0.0007205494453645122,
            -0.0018232088707029932,
            0.0056114348193944995,
            0.023680171946334084,
            -0.0594344186464569,
            -0.0764885990783064,
            0.41700518442169254,
            0.8127236354455423,
            0.3861100668211622,
            -0.06737255472196302,
            -0.04146493678175915,
            0.016387336463522112
        };

        private static readonly double[] Coif3 =
        {
            -3.459977283621256e-05,
            -7.098330313814125e-05,
            0.0004662169601128863,
            0.0011175187708906016,
            -0.0025745176887502236,
            -0.00900797613666158,
            0.015880544863615904,
            0.03455502757306163,
            -0.08230192710688598,
            -0.07179982161931202,
            0.42848347637761874,
            0.7937772226256206,
            0.4051769024096169,
            -0.06112339000267287,
            -0.0657719112818555,
            0.023452696141836267,
            0.007782596427325418,
            -0.003793512864491014
        };

        private static readonly double[] Coif4 =
        {
            -1.7849850030882614e-06,
            -3.2596802368833675e-06,
            3.1229875865345646e-05,
            6.233903446100713e-05,
            -0.00025997455248771324,
            -0.0005890207562443383,
            0.0012665619292989445,
            0.003751436157278457,
            -0.00565828668661072,
            -0.015211731527946259,
            0.025082261844864097,
            0.03933442712333749,
            -0.09622044203398798,
            -0.06662747426342504,
            0.4343860564914685,
            0.782238930920499,
            0.41530840703043026,
            -0.05607731331675481,
            -0.08126669968087875,
            0.026682300156053072,
            0.016068943964776348,
            -0.0073461663276420935,
            -0.0016294920126017326,
            0.0008923136685823146
        };

        private static readonly double[] Coif5 =
        {
            -9.517657273819165e-08,
            -1.6744288576823017e-07,
            2.0637618513646814e-06,
            3.7346551751414047e-06,
            -2.1315026809955787e-05,
            -4.134043227251251e-05,
            0.00014054114970203437,
            0.00030225958181306315,
            -0.0006381313430451114,
            -0.0016628637020130838,
            0.0024333732126576722,
            0.006764185448053083,
            -0.009164231162481846,
            -0.01976177894257264,
            0.03268357426711183,
            0.0412892087501817,
            -0.10557420870333893,
            -0.06203596396290357,
            0.4379916261718371,
            0.7742896036529562,
            0.4215662066908515,
            -0.05204316317624377,
            -0.09192001055969624,
            0.02816802897093635,
            0.023408156785839195,
            -0.010131117519849788,
            -0.004159358781386048,
            0.0021782363581090178,
            0.00035858968789573785,
            -0.00021208083980379827,
        };

        #endregion

        #region Вейвлет Мейера
        private static readonly double[] Meyer =
        {
            0.0,
            -1.009999956941423e-12,
            8.519459636796214e-09,
            -1.111944952595278e-08,
            -1.0798819539621958e-08,
            6.066975741351135e-08,
            -1.0866516536735883e-07,
            8.200680650386481e-08,
            1.1783004497663934e-07,
            -5.506340565252278e-07,
            1.1307947017916706e-06,
            -1.489549216497156e-06,
            7.367572885903746e-07,
            3.20544191334478e-06,
            -1.6312699734552807e-05,
            6.554305930575149e-05,
            -0.0006011502343516092,
            -0.002704672124643725,
            0.002202534100911002,
            0.006045814097323304,
            -0.006387718318497156,
            -0.011061496392513451,
            0.015270015130934803,
            0.017423434103729693,
            -0.03213079399021176,
            -0.024348745906078023,
            0.0637390243228016,
            0.030655091960824263,
            -0.13284520043622938,
            -0.035087555656258346,
            0.44459300275757724,
            0.7445855923188063,
            0.44459300275757724,
            -0.035087555656258346,
            -0.13284520043622938,
            0.030655091960824263,
            0.0637390243228016,
            -0.024348745906078023,
            -0.03213079399021176,
            0.017423434103729693,
            0.015270015130934803,
            -0.011061496392513451,
            -0.006387718318497156,
            0.006045814097323304,
            0.002202534100911002,
            -0.002704672124643725,
            -0.0006011502343516092,
            6.554305930575149e-05,
            -1.6312699734552807e-05,
            3.20544191334478e-06,
            7.367572885903746e-07,
            -1.489549216497156e-06,
            1.1307947017916706e-06,
            -5.506340565252278e-07,
            1.1783004497663934e-07,
            8.200680650386481e-08,
            -1.0866516536735883e-07,
            6.066975741351135e-08,
            -1.0798819539621958e-08,
            -1.111944952595278e-08,
            8.519459636796214e-09,
            -1.009999956941423e-12
        };
        #endregion

        #region Биортогональные

        private static readonly double[] Biorth1_1 =
        {
            0.7071067811865476,
            0.7071067811865476,
        };

        private static readonly double[] Biorth1_3 =
        {
            -0.08838834764831845,
            0.08838834764831845,
            0.7071067811865476,
            0.7071067811865476,
            0.08838834764831845,
            -0.08838834764831845,
        };

        private static readonly double[] Biorth1_5 =
        {
            0.01657281518405971,
            -0.01657281518405971,
            -0.12153397801643787,
            0.12153397801643787,
            0.7071067811865476,
            0.7071067811865476,
            0.12153397801643787,
            -0.12153397801643787,
            -0.01657281518405971,
            0.01657281518405971
        };

        private static readonly double[] Biorth2_2 =
        {
            0.0,
            -0.1767766952966369,
            0.3535533905932738,
            1.0606601717798214,
            0.3535533905932738,
            -0.1767766952966369
        };

        private static readonly double[] Biorth2_4 =
        {
            0.0,
            0.03314563036811942,
            -0.06629126073623884,
            -0.1767766952966369,
            0.4198446513295126,
            0.9943689110435825,
            0.4198446513295126,
            -0.1767766952966369,
            -0.06629126073623884,
            0.03314563036811942
        };

        private static readonly double[] Biorth2_6 =
        {
            0.0,
            -0.006905339660024878,
            0.013810679320049757,
            0.046956309688169176,
            -0.10772329869638811,
            -0.16987135563661201,
            0.4474660099696121,
            0.966747552403483,
            0.4474660099696121,
            -0.16987135563661201,
            -0.10772329869638811,
            0.046956309688169176,
            0.013810679320049757,
            -0.006905339660024878
        };

        private static readonly double[] Biorth2_8 =
        {
            0.0,
            0.0015105430506304422,
            -0.0030210861012608843,
            -0.012947511862546647,
            0.02891610982635418,
            0.052998481890690945,
            -0.13491307360773608,
            -0.16382918343409025,
            0.4625714404759166,
            0.9516421218971786,
            0.4625714404759166,
            -0.16382918343409025,
            -0.13491307360773608,
            0.052998481890690945,
            0.02891610982635418,
            -0.012947511862546647,
            -0.0030210861012608843,
            0.0015105430506304422
        };

        private static readonly double[] Biorth3_1 =
        {
            -0.3535533905932738,
            1.0606601717798214,
            1.0606601717798214,
            -0.3535533905932738
        };

        private static readonly double[] Biorth3_3 =
        {
            0.06629126073623884,
            -0.19887378220871652,
            -0.15467960838455727,
            0.9943689110435825,
            0.9943689110435825,
            -0.15467960838455727,
            -0.19887378220871652,
            0.06629126073623884
        };

        private static readonly double[] Biorth3_5 =
        {
            -0.013810679320049757,
            0.04143203796014927,
            0.052480581416189075,
            -0.26792717880896527,
            -0.07181553246425874,
            0.966747552403483,
            0.966747552403483,
            -0.07181553246425874,
            -0.26792717880896527,
            0.052480581416189075,
            0.04143203796014927,
            -0.013810679320049757
        };

        private static readonly double[] Biorth3_7 =
        {
            0.0030210861012608843,
            -0.009063258303782653,
            -0.01683176542131064,
            0.074663985074019,
            0.03133297870736289,
            -0.301159125922835,
            -0.026499240945345472,
            0.9516421218971786,
            0.9516421218971786,
            -0.026499240945345472,
            -0.301159125922835,
            0.03133297870736289,
            0.074663985074019,
            -0.01683176542131064,
            -0.009063258303782653,
            0.0030210861012608843
        };

        private static readonly double[] Biorth3_9 =
        {
            -0.000679744372783699,
            0.002039233118351097,
            0.005060319219611981,
            -0.020618912641105536,
            -0.014112787930175846,
            0.09913478249423216,
            0.012300136269419315,
            -0.32019196836077857,
            0.0020500227115698858,
            0.9421257006782068,
            0.9421257006782068,
            0.0020500227115698858,
            -0.32019196836077857,
            0.012300136269419315,
            0.09913478249423216,
            -0.014112787930175846,
            -0.020618912641105536,
            0.005060319219611981,
            0.002039233118351097,
            -0.000679744372783699
        };

        private static readonly double[] Biorth4_4 =
        {
            0.0,
            0.03782845550726404,
            -0.023849465019556843,
            -0.11062440441843718,
            0.37740285561283066,
            0.8526986790088938,
            0.37740285561283066,
            -0.11062440441843718,
            -0.023849465019556843,
            0.03782845550726404
        };

        private static readonly double[] Biorth5_5 =
        {
            0.0,
            0.0,
            0.03968708834740544,
            0.007948108637240322,
            -0.05446378846823691,
            0.34560528195603346,
            0.7366601814282105,
            0.34560528195603346,
            -0.05446378846823691,
            0.007948108637240322,
            0.03968708834740544,
            0.0
        };

        private static readonly double[] Biorth6_8 =
        {
            0.0,
            0.0019088317364812906,
            -0.0019142861290887667,
            -0.016990639867602342,
            0.01193456527972926,
            0.04973290349094079,
            -0.07726317316720414,
            -0.09405920349573646,
            0.4207962846098268,
            0.8259229974584023,
            0.4207962846098268,
            -0.09405920349573646,
            -0.07726317316720414,
            0.04973290349094079,
            0.01193456527972926,
            -0.016990639867602342,
            -0.0019142861290887667,
            0.0019088317364812906
        };

        #endregion


        public static int Delta => CH.Count - 2;

        //public enum WaveletType : int { Haar = 0, D4 = 1, D6 = 2, D8 = 3, D10 = 4}

        private static WaveletType _type;

        public static WaveletType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                GetCl();
                GetCh();
                if (value == WaveletType.Haar || value == WaveletType.D4 || value == WaveletType.D6 ||
                    value == WaveletType.D8 || value == WaveletType.D10 || value == WaveletType.D12 ||
                    value == WaveletType.D14 || value == WaveletType.D16 || value == WaveletType.D18 ||
                    value == WaveletType.D20)
                    SetIrregular();
                else GetReverse();
                //GetReverse();
            }
        }

        private static void SetIrregular()
        {
            var chArray = CH.ToArray();
            var clArray = CL.ToArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                chArray[i] /= Math.Sqrt(2);
                clArray[i] /= Math.Sqrt(2);
            }
            CH = chArray.ToList();
            CL = clArray.ToList();

            GetReverse();
        }

        //низкочастотный фильтр
        private static List<double> CL { get; set; }
        
        //высокочастотный фильтр
        private static List<double> CH { get; set; } 

        private static List<double> ReverseCL { get; set; }
        private static List<double> ReverseCH { get; set; } 

        //высокочастотный фильтр, в зависимости от настроек
        private static void GetCl()
        {
            switch (Type)
            {
                case WaveletType.Haar:
                    CL = Haar.ToList();
                    break;
                case WaveletType.D4:
                    CL = D4.ToList();
                    break;
                case WaveletType.D6:
                    CL = D6.ToList();
                    break;
                case WaveletType.D8:
                    CL = D8.ToList();
                    break;
                case WaveletType.D10:
                    CL = D10.ToList();
                    break;
                case WaveletType.D12:
                    CL = D12.ToList();
                    break;
                case WaveletType.D14:
                    CL = D14.ToList();
                    break;
                case WaveletType.D16:
                    CL = D16.ToList();
                    break;
                case WaveletType.D18:
                    CL = D18.ToList();
                    break;
                case WaveletType.D20:
                    CL = D20.ToList();
                    break;
                case WaveletType.Sym2:
                    CL = Sym2.ToList();
                    break;
                case WaveletType.Sym3:
                    CL = Sym3.ToList();
                    break;
                case WaveletType.Sym4:
                    CL = Sym4.ToList();
                    break;
                case WaveletType.Sym5:
                    CL = Sym5.ToList();
                    break;
                case WaveletType.Sym6:
                    CL = Sym6.ToList();
                    break;
                case WaveletType.Sym7:
                    CL = Sym7.ToList();
                    break;
                case WaveletType.Sym8:
                    CL = Sym8.ToList();
                    break;
                case WaveletType.Sym9:
                    CL = Sym9.ToList();
                    break;
                case WaveletType.Sym10:
                    CL = Sym10.ToList();
                    break;
                case WaveletType.Coif1:
                    CL = Coif1.ToList();
                    break;
                case WaveletType.Coif2:
                    CL = Coif2.ToList();
                    break;
                case WaveletType.Coif3:
                    CL = Coif3.ToList();
                    break;
                case WaveletType.Coif4:
                    CL = Coif4.ToList();
                    break;
                case WaveletType.Coif5:
                    CL = Coif5.ToList();
                    break;
                case WaveletType.Meyer:
                    CL = Meyer.ToList();
                    break;
                case WaveletType.Biorth1_1:
                    CL = Biorth1_1.ToList();
                    break;
                case WaveletType.Biorth1_3:
                    CL = Biorth1_3.ToList();
                    break;
                case WaveletType.Biorth1_5:
                    CL = Biorth1_5.ToList();
                    break;
                case WaveletType.Biorth2_2:
                    CL = Biorth2_2.ToList();
                    break;
                case WaveletType.Biorth2_4:
                    CL = Biorth2_4.ToList();
                    break;
                case WaveletType.Biorth2_6:
                    CL = Biorth2_6.ToList();
                    break;
                case WaveletType.Biorth2_8:
                    CL = Biorth2_8.ToList();
                    break;
                case WaveletType.Biorth3_1:
                    CL = Biorth3_1.ToList();
                    break;
                case WaveletType.Biorth3_3:
                    CL = Biorth3_3.ToList();
                    break;
                case WaveletType.Biorth3_5:
                    CL = Biorth3_7.ToList();
                    break;
                case WaveletType.Biorth3_9:
                    CL = Biorth3_9.ToList();
                    break;
                case WaveletType.Biorth4_4:
                    CL = Biorth4_4.ToList();
                    break;
                case WaveletType.Biorth5_5:
                    CL = Biorth5_5.ToList();
                    break;
                case WaveletType.Biorth6_8:
                    CL = Biorth6_8.ToList();
                    break;
            }
        }

        //генерация второй строчки - высокочастотного фильтра (коэф. в обратном порядке с чередованием знаков)
        private static void GetCh()
        {
            CL.Reverse();
            CH = new List<double>();
            for (var i=0; i<CL.Count; i++)
                CH.Add(Math.Pow(-1, i) * CL[i]);
            CL.Reverse();
        }

        private static void GetReverse()
        {
            CL.Reverse();
            CH.Reverse();
            ReverseCL = new List<double>();
            ReverseCH = new List<double>();

            //формируем первый вектор
            for (int i = 1; i < CH.Count; i+=2)
            {
                ReverseCL.Add(CL[i]);
                ReverseCL.Add(CH[i]);
            }

            //формируем второй вектор
            for (int i = 0; i < CH.Count; i += 2)
            {
                ReverseCH.Add(CL[i]);
                ReverseCH.Add(CH[i]);
            }
            CL.Reverse();
            CH.Reverse();
        }

        //чтобы по  индексу -1 можно было получать доступ к последнему элементу (-2 предпоследний, и т.д.)
        private static int GetIndex(int index, int length)
        {
            if (index >= 0) return index%length;
            var ind = (length + index)%length;
            if (ind > 0) return ind;
            return GetIndex(ind, length);
        }

        public static void Convert(List<short> data, out List<double> slList, out List<double> shList, int delta = 0)
        {
            slList = new List<double>();
            shList = new List<double>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)]*CL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] *CH[j];
                }

                slList.Add(sL);
                shList.Add(sH);
            }
        }

        public static void Convert(List<short> data, out List<double> outList, int delta = 0)
        {
            outList = new List<double>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * CL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * CH[j];
                }

                outList.Add(sL);
                outList.Add(sH);
            }
        }

        public static List<short> Deconvert(List<double> data, int delta = 0)
        {
            var outList = new List<short>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * ReverseCL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * ReverseCH[j];
                }

                outList.Add((short)Math.Round(sL));
                outList.Add((short)Math.Round(sH));
            }
            return outList;
        }

        public static List<short> Deconvert(List<short> data, int delta = 0)
        {
            var outList = new List<short>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * ReverseCL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * ReverseCH[j];
                }

                outList.Add((short)Math.Round(sL));
                outList.Add((short)Math.Round(sH));
            }
            return outList;
        }


        #region матрицы
        private static readonly double[][] KoefMatrix = new[]
        {
            new double[]
            {
                (1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2))
            },
            new double[]
            {
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(1 + Math.Sqrt(3))/(4*Math.Sqrt(2))
            }
        };

        private static readonly double[][] IkoefMatrix = new[]
        {
            new double[]
            {
                (3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2))
            },
            new double[]
            {
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
            }
        };
        #endregion

        public static List<double> Encode(short[] data, int delta, out List<double> sL, out List<double> sH)
        {
            var rez = new List<double>();
            sL = new List<double>();
            sH = new List<double>();

            for (int i = 0; i < data.Length; i += 2)
            {
                double[] vectorKoef = new double[2]; //0 - СL, 1 - CH 
                for (int j = 0; j < 4; j++)
                {
                    vectorKoef[0] += data[(i + j - delta) % data.Length] * KoefMatrix[0][j];
                    vectorKoef[1] += data[(i + j - delta) % data.Length] * KoefMatrix[1][j];
                }
                sL.Add(vectorKoef[0]);
                sH.Add(vectorKoef[1]);
                rez.Add(vectorKoef[0]);
                rez.Add(vectorKoef[1]);
            }
            return rez;
        }

        public static List<double> Decode(double[] data, int delta, out List<double> sL, out List<double> sH)
        {
            var rez = new List<double>();
            sL = new List<double>();
            sH = new List<double>();

            for (int i = 0; i < data.Length; i += 2)
            {
                double[] vectorKoef = new double[2]; //0 - СL, 1 - CH 
                for (int j = 0; j < 4; j++)
                {
                    vectorKoef[0] += data[(i + j + delta) % data.Length] * IkoefMatrix[0][j];
                    vectorKoef[1] += data[(i + j + delta) % data.Length] * IkoefMatrix[1][j];
                }
                sL.Add(vectorKoef[0]);
                sH.Add(vectorKoef[1]);
                rez.Add(vectorKoef[0]);
                rez.Add(vectorKoef[1]);
            }
            return rez;
        }
    }
}
