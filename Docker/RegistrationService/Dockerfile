FROM debian:stable

MAINTAINER Stefan Thoeni, Piratenpartei Schweiz

RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get -y install git mono-complete
RUN DEBIAN_FRONTEND=noninteractive apt-get -y install mono-xsp
RUN git clone https://github.com/ppschweiz/MemberDatabase
RUN cp -r ./MemberDatabase/Organigram/RegistrationService .
RUN rm -r /MemberDatabase

COPY entrypoint.sh /entrypoint.sh
COPY member.config.aes /etc/member.config.aes
COPY Web.config /RegistrationService/Web.config
RUN mkdir /var/log/mdb

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
CMD ["/usr/bin/xsp --port 80 --root /RegistrationService --nonstop"]

