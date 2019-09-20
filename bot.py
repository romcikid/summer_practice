import pymongo
import telebot
import os
from flask import Flask, request
from pymongo import MongoClient

TOKEN = "956639436:AAGmIY1N83XTVRoEqyhrpkuBekSPn8vrYI0"
bot = telebot.TeleBot("956639436:AAGmIY1N83XTVRoEqyhrpkuBekSPn8vrYI0")

server = Flask(__name__)

#   Подключение к базе данных.
client = MongoClient("mongodb+srv://Reader:111@littlebrother-bp44k.mongodb.net/test?retryWrites=true&w=majority")
db = client.get_database('Data_for_app')

#   Получение коллекции.
collection = db.employee

#   Действие при старте бота.
@bot.message_handler(commands=['start'])
def start(message):
    user_markup = telebot.types.ReplyKeyboardMarkup(True,False)
    user_markup.row('/start','/info')
    start_text = str('Привет, '+message.from_user.first_name+'!\nЯ бот-поисковик.'+
                     '\nЧтобы найти сотрудника напиши мне его ФИО.' +
                     'Например: Иванов Иван Иванович')
    bot.send_message(text=start_text, parse_mode='Markdown')


#   Действие при получении текста от пользователя.
@bot.message_handler(commands=['text'])
def get_text(message):

    #   Если человек присутствует в базе данных.
    if collection.find_one({"Name": message.text}):
        result = collection.find_one({"Name": message.text})
        text = 'ФИО:' + result["Name"] + '\nДолжность: ' + result["Position"] + '\nМестоположение: ' + result["Location"] + bot.send_message(text=text, parse_mode='Markdown')

    #   И если человек отстутствует в базе данных.
    else:
        bot.send_message('Простите, данного человека нет в базе данных. Попробуйте ещё раз)',
                         parse_mode='Markdown')

@server.route('/' + TOKEN, methods=['POST'])
def getMessage():
    bot.process_new_updates([telebot.types.Update.de_json(request.stream.read().decode("utf-8"))])
    return "!", 200

@server.route("/")
def webhook():
    bot.remove_webhook()
    bot.set_webhook(url='https://l1ttlebrother.herokuapp.com/' + TOKEN)
    return "!", 200


if __name__ == '__main__':
    server.debug = True
    server.run(host="0.0.0.0", port=int(os.environ.get('PORT', 5000)))