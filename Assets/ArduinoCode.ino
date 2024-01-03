#include <Arduino.h>
#include <WiFi.h>
#include <FirebaseESP32.h>
#include <DHT11.h>
#include <movingAvg.h>
#include <addons/TokenHelper.h>
#include <addons/RTDBHelper.h>

/* 1. Define the WiFi credentials */
#define WIFI_SSID "Fabis Phone"
#define WIFI_PASSWORD "hochsicher31"
#define API_KEY "AIzaSyCsZd1Ag-KsjkQWU0XA_9zBM4CCudgmuf4"
#define USER_EMAIL "mail@fabianschmid.co"
#define USER_PASSWORD "password"
#define DATABASE_URL "https://ntut-web-lab-default-rtdb.asia-southeast1.firebasedatabase.app"

FirebaseData fbdo;
FirebaseAuth auth;
FirebaseConfig config;

unsigned long dataMillis = 0;
unsigned int secondCount = 0;
int count = 0;
String path;
int dhtPin = 32;
DHT11 dht11(dhtPin);

movingAvg tempAvg(10);
movingAvg humAvg(10);

void setup()
{
    Serial.begin(115200);

    // WIFI
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
    Serial.print("Connecting to Wi-Fi");
    while (WiFi.status() != WL_CONNECTED)
    {
        Serial.print(".");
        delay(300);
    }
    Serial.println();
    Serial.print("Connected with IP: ");
    Serial.println(WiFi.localIP());
    Serial.println();

    // GPIO
    tempAvg.begin();
    humAvg.begin();
    
    // FIREBASE
    Serial.printf("Firebase Client v%s\n\n", FIREBASE_CLIENT_VERSION);
    config.api_key = API_KEY;
    auth.user.email = USER_EMAIL;
    auth.user.password = USER_PASSWORD;
    config.database_url = DATABASE_URL;
    Firebase.reconnectNetwork(true);
    fbdo.setBSSLBufferSize(4096, 1024);
    fbdo.setResponseSize(4096);

    path = "/rooms/0/sensors/0/measurements";

    config.token_status_callback = tokenStatusCallback; // see addons/TokenHelper.h
    Serial.println("Signing in as user mail@fabianschmid.co");

    Firebase.begin(&config, &auth);
}

void loop()
{
    if (millis() - dataMillis > 1000)
    {    
      dataMillis = millis();
      secondCount++;
      
      int temperature = random(18,23);//dht11.readTemperature();
      delay(50);
      int humidity = random(50,65);//dht11.readHumidity();
      int tempAvgVal;
      int humAvgVal;

      bool tempValid = true;
      bool humValid = true;

      if (temperature != DHT11::ERROR_CHECKSUM && temperature != DHT11::ERROR_TIMEOUT &&
        humidity != DHT11::ERROR_CHECKSUM && humidity != DHT11::ERROR_TIMEOUT)
      {
        Serial.print(String(temperature) + " °C, " + String(humidity) + " %");

        tempAvgVal = tempAvg.reading(temperature);
        humAvgVal =  humAvg.reading(humidity);

        Serial.println(String(tempAvgVal) + " °C, " + String(humAvgVal) + "%");
      }
      else
      {
          if (temperature == DHT11::ERROR_TIMEOUT || temperature == DHT11::ERROR_CHECKSUM)
          {
              tempValid = false;
              Serial.print("Temperature Reading Error: ");
              Serial.println(DHT11::getErrorString(temperature));
          }
          if (humidity == DHT11::ERROR_TIMEOUT || humidity == DHT11::ERROR_CHECKSUM)
          {
              humValid = false;
              Serial.print("Humidity Reading Error: ");
              Serial.println(DHT11::getErrorString(humidity));
          }
      }

      // send to server
      if (secondCount >= 10 && Firebase.ready()) {
        secondCount = 0;
        if (tempValid) {
          Serial.printf("Set temperature for sensor 0 in room 0... %s\n", Firebase.setInt(fbdo, path+"/temperature", temperature) ? "ok" : fbdo.errorReason().c_str());
        }
        if (humValid) {
          Serial.printf("Set humidity for sensor 0 in room 0... %s\n", Firebase.setInt(fbdo, path+"/humidity", humidity) ? "ok" : fbdo.errorReason().c_str());
        }
      }
   
    }
    

    
}
