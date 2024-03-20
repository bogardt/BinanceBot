namespace BinanceBotML
{
    public class TradingBot
    {
        private AnalyzerML _analyzer;

        public TradingBot(string csvPath)
        {
            _analyzer = new AnalyzerML();
            _analyzer.Train2(csvPath);
        }

        public float MakeTradingDecision(float currentClosePrice)
        {

            // Simulez les données de marché pour la prédiction actuelle
            var marketData = new MarketData
            {
                Close = currentClosePrice,

                // Ajoutez les autres champs requis par votre modèle ici
            };

            var prediction = _analyzer.MLContext.Model.CreatePredictionEngine<MarketData, MarketPrediction>(_analyzer.Model).Predict(marketData);
            Console.WriteLine($"Predicted Future Close: {prediction.Close}");

            // Décision de trading basée sur la prédiction
            if (prediction.Close > currentClosePrice)
            {
                Console.WriteLine("Decision: Buy");
                // Implémentez la logique d'achat
            }
            else
            {
                Console.WriteLine("Decision: Sell");
                // Implémentez la logique de vente
            }
            Console.WriteLine($"prediction close : {prediction.Close}");
            return prediction.Close;
        }



    }
}
