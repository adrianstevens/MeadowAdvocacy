﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace LineChart
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            Console.WriteLine("Init complete");

            return base.Initialize();
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            double yScale = 75;

            graphics.Clear();

            for (int i = 0; i < data.Length - 1; i++)
            {
                graphics.DrawLine(data[i].Item1, (int)(data[i].Item2 * yScale), data[i + 1].Item1, (int)(data[i + 1].Item2 * yScale), Color.Cyan);
            }

            graphics.Show();

            return base.Run();
        }

        public static (int, double)[] data = new (int, double)[]
        {
            (0,2.46869580144828),(1,1.02995394044899),(2,1.23434790072414),(3,1.31099563582732),(4,1.38205447357923),
            (5,1.58006112259578),(6,1.64792630471838),(7,1.72936452326551),(8,1.81319798353462),(9,1.91539496367219),
            (10,1.98166331798015),(11,2.09423967891295),(12,2.14933023851836),(13,2.23635735441676),(14,2.29943205309542),
            (15,2.38725758290115),(16,2.46470373191165),(17,2.46230849018968),(18,2.46869580144828),(19,2.50701966899987),
            (20,2.49584187429732),(21,2.50941491072184),(22,2.50382601337057),(23,2.44554179813586),(24,2.35931309614478),
            (25,2.27547963587568),(26,2.1796699669967),(27,2.06869043387855),(28,1.92497593056009),(29,1.8028186027394),
            (30,1.67347554975278),(31,1.54173725504419),(32,1.41319261596489),(33,1.21997645039229),(34,1.11378740071809),
            (35,1.01398566230249),(36,0.936539513291989),(37,0.880650539779253),(38,0.876658470242629),(39,0.863085433818107),
            (40,0.849512397393586),(41,0.852706053022885),(42,0.961290344419057),(43,1.04751904641014),(44,1.18404782456268),
            (45,1.34692426165693),(46,1.48984035106807),(47,1.65750727160628),(48,1.80122177492475),(49,2.03675387758556),
            (50,2.17487948355275),(51,2.29943205309542),(52,2.41440365575019),(53,2.46550214581898),(54,2.48306725178012),
            (55,2.49903552992662),(56,2.49105139085337),(57,2.3952417219744),(58,2.28745584448555),(59,2.13895085772313),
            (60,1.95052517559448),(61,1.74932487094863),(62,1.55850394709801),(63,1.3748687484133),(64,1.19841927489452),
            (65,0.993226900712049),(66,0.91338550997957),(67,0.878255298057278),(68,0.857496536466833),(69,0.847117155671611),
            (70,0.974064966936254),(71,1.12336836760599),(72,1.32776232788114),(73,1.54812456630278),(74,1.76209949346583),
            (75,1.96329979811168),(76,2.15891120540626),(77,2.33136860938841),(78,2.43675924515529),(79,2.4790751822435),
            (80,2.49983394383394),(81,2.49743870211197),(82,2.34813530144223),(83,2.1796699669967),(84,1.94812993387251),
            (85,1.69263748352857),(86,1.48025938418018),(87,1.23913838416809),(88,1.05151111594676),(89,0.921369649052817),
            (90,0.875061642427979),(91,0.843125086134987),(92,0.903006129184347),(93,1.06747939409326),(94,1.30860039410534),
            (95,1.69104065571392),(96,1.95771090076041),(97,2.20601762593842),(98,2.39444330806707),(99,2.48386566568745),
            (100,2.49105139085337),(101,2.49983394383394),(102,2.35611944051548),(103,2.13655561600116),(104,1.85551392062283),
            (105,1.5712785696152),(106,1.31897977490057),(107,1.09223022522032),(108,0.910990268257595),(109,0.880650539779253),
            (110,0.857496536466833),(111,0.942128410643262),(112,1.18245099674803),(113,1.44433075835056),(114,1.73415500670946),
            (115,2.03595546367824),(116,2.2858590166709),(117,2.45192910939446),(118,2.47188945707758),(119,2.49743870211197),
            (120,2.34973212925688),(121,2.11100637096677),(122,1.7996249471101),(123,1.50740545702922),(124,1.19841927489452),
            (125,0.95170937753116),(126,0.87186798679868),(127,0.843125086134987),(128,0.947717307994536),(129,1.22476693383624),
            (130,1.55451187756138),(131,1.85790916234481),(132,2.14933023851836),(133,2.40162903323299),(134,2.4790751822435),
            (135,2.50382601337057),(136,2.35053054316421),(137,2.08226347030307),(138,1.73175976498749),(139,1.42516882457477),
            (140,1.1010127782009),(141,0.91338550997957),(142,0.873464814613329),(143,0.877456884149953),(144,1.09382705303497),
            (145,1.41638627159419),(146,1.77886618551965),(147,2.23635735441676),(148,2.463106904097),(149,2.50302759946324),
            (150,2.46550214581898),(151,2.22837321534351),(152,1.88265999347188),(153,1.51858325173177),(154,1.17047478813815),
            (155,0.914982337794219),(156,0.860690192096133),(157,0.875860056335304),(158,1.12416678151332),(159,1.50261497358527),
            (160,1.86349805969608),(161,2.21400176501167),(162,2.45831642065305),(163,2.49105139085337),(164,2.42877510608204),
            (165,2.14054768553778),(166,1.75172011267061),(167,1.34612584774961),(168,1.03394600998561),(169,0.885441023223201),
            (170,0.855101294744859),(171,1.02196980137574),(172,1.3908370265598),(173,1.78844715240755),(174,2.16849217229415),
            (175,2.44394497032121),(176,2.48785773522407),(177,2.3888544107158),(178,2.08386029811772),(179,1.65591044379163),
            (180,1.25909873185121),(181,0.962887172233707),(182,0.867077503354731),(183,0.882247367593902),(184,1.19362879145057),
            (185,1.64872471862571),(186,2.05032691401008),(187,2.40083061932567),(188,2.49424504648267),(189,2.36889406303268),
            (190,2.01599511599512),(191,1.56968174180055),(192,1.16009540734293),(193,0.887836264945176),(194,0.85190763911556),
            (195,1.01877614574644),(196,1.39802275172572),(197,1.87148219876933),(198,2.28665743057822),(199,2.48386566568745),
            (200,2.50382601337057),(201,2.22597797362154),(202,1.77247887426105),(203,1.33734329476904),(204,0.965282413955681),
            (205,0.871069572891355),(206,0.925361718589441),(207,1.27426859609038),(208,1.76289790737316),(209,2.21240493719702),
            (210,2.46550214581898),(211,2.50302759946324),(212,2.28106853322695),(213,1.81958529479322),(214,1.34293219212031),
            (215,0.95011254971651),(216,0.859891778188808),(217,0.963685586141032),(218,1.39323226828177),(219,1.8834584073792),
            (220,2.31619874514924),(221,2.50302759946324),(222,2.36729723521803),(223,1.93695213916996),(224,1.44353234444324),
            (225,1.01238883448784),(226,0.867077503354731),(227,0.927756960311416),(228,1.33654488086171),(229,1.83954564247634),
            (230,2.28985108620752),(231,2.48306725178012),(232,2.4567195928384),(233,2.05431898354671),(234,1.54413249676616),
            (235,1.07466511925918),(236,0.873464814613329),(237,0.906199784813646),(238,1.28384956297828),(239,1.85551392062283),
            (240,2.31779557296389),(241,2.48626090740942),(242,2.40322586104764),(243,1.98246173188747),(244,1.42676565238941),
            (245,0.982847519916827),(246,0.847915569578936),(247,1.00999359276587),(248,1.50660704312189),(249,2.04314118884416),
            (250,2.43755765906261),(251,2.51420539416579),(252,2.11739368222537),(253,1.55850394709801),(254,1.06348732455663)
        };

    }
}