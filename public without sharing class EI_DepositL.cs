public without sharing class EI_DepositLookupapex {
    //test fda
    @AuraEnabled 
    Public static List<Case> getChangeOverCase(){
    /*  User tenantAccount = [Select contact.AccountId,id from user where id=: UserInfo.getUserId()];
        set<Id> AccountId = new set<Id>();
        set<Id> DepositId = new set<Id>();
        for(Deposit_Allocation__c dplist :[Select id,Deposit_Holder__r.id,Deposit__r.id,Istenantmoved__c from Deposit_Allocation__c where Istenantmoved__c = true and Deposit_Holder__r.id =: tenantAccount.contact.AccountId]){
            AccountId.add(dplist.Deposit_Holder__r.id);
            DepositId.add(dplist.Deposit__r.id);
        }
       
        
        List<Case> caseList = [Select id,Subject,Deposit_Account_Number__r.Deposit_Account_Number__c,ownerid,Status from Case where  (AccountId in: AccountId OR Deposit_Account_Number__c in: DepositId) and Status = 'Tenant changeover' and ChangeOver_Status__c = 'Agent Approved'];
        if(caseList.size()>1){
             return caseList;
        }else{

            singlist.add(caseList[0]);
      return singlist;
        }*/           
        List<Case> singlist = new List<Case>();
      //  singlist= EI_TenentChangeoverApx.getChangeOverCase();
        return singlist;
    }
  @AuraEnabled
    public static list<Deposit_Allocation__c> loggedintenantdeposits(){
        user userrec = [select id,FirstName, LastName, Email,AccountId, ContactId from user where id = :UserInfo.getUserId()];
        list<Deposit_Allocation__c> Depositlist =[select id,Deposit__r.Active__c,Deposit__c, Deposit_Status__c,
                        Deposit_Holder__r.personemail from Deposit_Allocation__c  
                        where Deposit_Holder__c IN (SELECT accountid from User where Id = :UserInfo.getUserId())
                                     and  Deposit_Holder__r.personemail=:userrec.Email /*and Deposit_Status__c=:Label.Validated */] ;  
        return Depositlist; 
    }
    
    
    @AuraEnabled
	public static Account loggedInUserAccountInfo(){
        try{
        Account Acc = [SELECT Id,PersonEmail, Alternative_Email__pc
		               FROM Account where Id IN (SELECT AccountId
		                            from User
		                            where Id = :UserInfo.getUserId()) and  PersonEmail LIKE '%@ac.uk%' and  Alternative_Email__pc=null];
      return Acc;  
        }
        catch (Exception e){
			throw new AuraHandledException(e.getMessage());
		}
         
        }
     @AuraEnabled
    public static string updatealternateemail(Account Acc,string altenateemail){
     SavePoint sp = Database.setSavePoint();
        try{
            Acc.Alternative_Email__pc=altenateemail;
            update Acc;
        }
        catch(Exception e){
            Database.rollback(sp);
        } 
        return Acc.id;
    }
    
@AuraEnabled
    public static list<Deposit_Allocation__c> getdepositdetails (string DAN, string postcode, integer month,integer year,Decimal depositamount, string surname  ){
        system.debug('DAN' + DAN);
        system.debug('postcode' + postcode);
        system.debug('month' + month);
        system.debug('year' + year);
        system.debug('depositamount' + depositamount);
        system.debug('surname' + surname);
        list<Deposit_Allocation__c> depositlist = new list<Deposit_Allocation__c> ();
        if(DAN!=null){
        for (Deposit_Allocation__c depall1 : [select id,Deposit__c,role__c,Deposit__r.Deposit_Amount__c,Deposit_Holder__r.FirstName,Deposit_Holder__r.LastName, Deposit__r.Start_Date__c,deposit__r.Propertypostalcode__c from Deposit_Allocation__c  where Deposit__r.name=:DAN  and Deposit__r.Original_Deposit_Amount__c=:depositamount and Deposit_Holder__r.LastName=:surname and Deposit__r.Status__c='Deposits held by scheme'and Deposit_Status__c='Unvalidated' and Role__c='Tenant' limit 1]){
            if((depall1.deposit__r.Propertypostalcode__c).trim().toLowercase().deleteWhitespace()==(postcode).trim().toLowercase().deleteWhitespace()){
             depositlist.add(depall1);    
            }
            
        }   
        }
        else{
        for (Deposit_Allocation__c depall2 : [select id,Deposit__c,role__c,Deposit__r.Deposit_Amount__c,Deposit_Holder__r.FirstName,Deposit_Holder__r.LastName, Deposit__r.Start_Date__c,deposit__r.Propertypostalcode__c from Deposit_Allocation__c  where    Deposit__r.Original_Deposit_Amount__c=:depositamount and Deposit_Holder__r.LastName=:surname and Deposit__r.Status__c='Deposits held by scheme' and Deposit_Status__c='Unvalidated'  and Role__c='Tenant' limit 1]){
            if((depall2.deposit__r.Propertypostalcode__c).trim().toLowercase().deleteWhitespace()==(postcode).trim().toLowercase().deleteWhitespace()){
             depositlist.add(depall2);    
            }
        }      
        }
        return depositlist;
    }
    
    @AuraEnabled
    public static string  addtenanttodeposit(string selecteddeposit){
        string message ='Deposit Linked Successfully';
        system.debug('line-->28' + selecteddeposit);
        list<Deposit_Allocation__c> depall = (List<Deposit_Allocation__c>) System.JSON.deserialize(selecteddeposit, List<Deposit_Allocation__c>.class);  
        user userrec = [select id,FirstName, LastName, Email,AccountId, ContactId from user where id = :UserInfo.getUserId()];
        system.debug('line-->31 ' + userrec);
        list<Deposit_Allocation__c> matchingtenant = [select id,Deposit__c,role__c,Deposit__r.Deposit_Amount__c,Deposit_Holder__r.Name,Deposit_Holder__r.FirstName,Deposit_Holder__r.LastName,Deposit_Holder__r.personemail from Deposit_Allocation__c where Deposit__c=:depall[0].Deposit__c and  Deposit_Holder__r.FirstName=:userrec.FirstName and Deposit_Holder__r.LastName=:userrec.LastName and role__c='Tenant' limit 1   ];               
        Deposit_Allocation__c attachtodeposit = new Deposit_Allocation__c();
        attachtodeposit.Deposit__c = depall[0].Deposit__c;
        attachtodeposit.Deposit_Holder__c=userrec.AccountId;
        attachtodeposit.Contact__c=userrec.ContactId;
        attachtodeposit.Role__c ='Tenant';
       // attachtodeposit.Deposit_Status__c='Validated';
        
        if(matchingtenant.size()>0 && matchingtenant[0].Deposit_Holder__r.personemail!=null ){
           mailtoremoveduser(matchingtenant[0].Deposit_Holder__r.personemail,matchingtenant[0].Deposit_Holder__r.Name);
          delete  matchingtenant; 
          insert attachtodeposit;
        }
        else if(matchingtenant.size()>0 && matchingtenant[0].Deposit_Holder__r.personemail==null){
           delete  matchingtenant; 
          insert attachtodeposit;  
            
        }
        else
        {
         insert attachtodeposit;      
        }
        
            
       return message;
     }
    
    @future(callout = true)
    public static void mailtoremoveduser(string userpersonemail,string username){ 
    string message = 'mail send';
        
        HttpRequest req = EI_mailJetServiceUtility.mailJetAuthentication();
        JSONGenerator gen = JSON.createGenerator(true);
        
        gen.writeStartObject();     
        gen.writeFieldName('Messages');
        gen.writeStartArray();
        gen.writeStartObject(); 
        gen.writeFieldName('From');
        gen.writeStartObject();
        gen.writeStringField('Email', 'ashish.singh1@espire.com');
        gen.writeStringField('Name', 'SDS');
        gen.writeEndObject();
        
        gen.writeFieldName('To');
        gen.writeStartArray();
        gen.writeStartObject(); 
        gen.writeStringField('Email', userpersonemail);    
        gen.writeStringField('Name', username);
        gen.writeEndObject();      
        gen.writeEndArray();
        gen.writeEndArray11();
        
        gen.writeNumberField('TemplateID', 2387020);
        gen.writeBooleanField('TemplateLanguage', true);
        gen.writeStringField('Subject', 'Test Email template');
        
        gen.writeFieldName('Variables');       
        gen.writeStartObject();  
        gen.writeStringField('date', system.today().format());
        gen.writeStringField('name', username);
        gen.writeEndObject(); 
        
        gen.writeEndObject();
        gen.writeEndArray();
        gen.writeEndObject(); 
             
        String jsonData = gen.getAsString(); 
        System.debug('jsonData- ' + jsonData);
        
        req.setBody(jsonData);
        Http http = new Http();
        HTTPResponse res = http.send(req);
        System.debug(res.getBody());
    }

}